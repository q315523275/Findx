using Findx.Data;
using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Scheduling;
using Findx.Jobs.Common;
using Findx.NewId;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Findx.Extensions.Scheduler.Storage;

/// <summary>
///     基于数据表的周期领取器。
///     <para>核心：同一个事务里先做"任务行 NextRunTime 列级 CAS"，成功后写执行行。
///     这样两个 Master 并发扫描时只有一个能提交，另一个在 CAS 处返回 0 行；
///     而"提交成功但随后进程崩溃"的情况，执行行已经落库，由超时兜底接管，不会静默丢失。</para>
/// </summary>
public class DatabasePeriodClaimer : IPeriodClaimer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IClusterClock _clock;
    private readonly IKeyGenerator<long> _keyGenerator;
    private readonly ILogger<DatabasePeriodClaimer> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="clock"></param>
    /// <param name="keyGenerator"></param>
    /// <param name="logger"></param>
    public DatabasePeriodClaimer(IServiceProvider serviceProvider, IClusterClock clock,
        IKeyGenerator<long> keyGenerator, ILogger<DatabasePeriodClaimer> logger)
    {
        _serviceProvider = serviceProvider;
        _clock = clock;
        _keyGenerator = keyGenerator;
        _logger = logger;
    }

    /// <summary>
    ///     批量领取：一批到期任务共用同一个事务与一次提交。
    ///     <para>逐行 CAS 判定与单条领取完全一致（CAS 未命中只跳过该行），区别在于：
    ///     一批 N 条只有 1 次 COMMIT、1 次批量 INSERT。压测显示单条领取的成本大头是
    ///     事务提交（含 redo 刷盘），批次越大摊薄越明显。</para>
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>与请求等长的结果集合（顺序与入参一致）</returns>
    public async Task<IReadOnlyList<ClaimBatchResult>> TryClaimBatchAsync(IReadOnlyList<ClaimBatchRequest> requests, CancellationToken cancellationToken = default)
    {
        if (requests == null || requests.Count == 0) return Array.Empty<ClaimBatchResult>();

        //  单条请求仍走原路径：批量路径的价值在摊薄提交，不值得为 1 条多开一层判断
        if (requests.Count == 1)
        {
            var single = requests[0];
            if (single?.Job == null || single.Periods == null || single.Periods.Count == 0)
                return [new ClaimBatchResult { Request = single, Executions = null }];

            var executions = single.Kind == DispatchKind.Retry
                ? await ClaimRetryAsListAsync(single, cancellationToken)
                : await TryClaimCycleAsync(single.Job, single.Periods, single.Attempt, single.ExpectedTime, single.NewNextRunTime, single.LastRunTime, cancellationToken);
            return [new ClaimBatchResult { Request = single, Executions = executions?.Count > 0 ? executions : null }];
        }

        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        await using var unitOfWork = await unitOfWorkManager.GetEntityUnitOfWorkAsync<ScheduledJob>(true, true, cancellationToken);

        //  结果槽位先按入参顺序占位，后面按组回填
        var results = new List<ClaimBatchResult>(requests.Count);
        for (var index = 0; index < requests.Count; index++)
            results.Add(new ClaimBatchResult { Request = requests[index], Executions = null });

        try
        {
            var jobRepository = unitOfWork.GetRepository<ScheduledJob, long>();

            //  按「同一 CAS 语义」分组：同周期锚点、同新锚点、同 LastRunTime 的任务可合成一条多行 UPDATE。
            //  典型场景就是同一波 cron 触发——2000 个任务原本 2000 条 UPDATE，现在 1 条。
            var groups = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            for (var index = 0; index < requests.Count; index++)
            {
                var request = requests[index];
                if (request?.Job == null || request.Periods == null || request.Periods.Count == 0) continue;

                var key = string.Concat(
                    request.Kind == DispatchKind.Retry ? "R" : "C", "|",
                    request.ExpectedTime.Ticks.ToString(), "|",
                    request.Kind == DispatchKind.Retry ? "0" : request.NewNextRunTime.Ticks.ToString(), "|",
                    request.Kind == DispatchKind.Retry ? "0" : request.LastRunTime.Ticks.ToString());

                if (!groups.TryGetValue(key, out var indices)) groups[key] = indices = new List<int>();
                indices.Add(index);
            }

            var pendingExecutions = new List<SchedulerExecution>(requests.Count);
            foreach (var indices in groups.Values)
            {
                var sample = requests[indices[0]];
                var claimed = new bool[indices.Count];

                if (indices.Count == 1)
                {
                    claimed[0] = await TryClaimOneAsync(jobRepository, sample, cancellationToken);
                }
                else
                {
                    var ids = new List<long>(indices.Count);
                    foreach (var index in indices) ids.Add(requests[index].Job.Id);

                    var affected = await TryClaimManyAsync(jobRepository, sample, ids, cancellationToken);
                    if (affected == indices.Count)
                    {
                        for (var i = 0; i < claimed.Length; i++) claimed[i] = true;
                    }
                    else
                    {
                        //  部分命中：一条多行 UPDATE 无法告诉我们具体命中哪几行，只能回查「锚点仍等于期望值」的行。
                        //  这些行才是本组没覆盖到的；其余行（含被其他节点抢走的）锚点已推进，必须视为已领取，
                        //  否则会重复建执行行或把「已推进但没执行行」的周期静默丢掉。
                        var pendingIds = await SelectPendingIdsAsync(jobRepository, sample, ids, cancellationToken);
                        for (var i = 0; i < indices.Count; i++)
                        {
                            var jobId = requests[indices[i]].Job.Id;
                            claimed[i] = !pendingIds.Contains(jobId);
                        }

                        for (var i = 0; i < indices.Count; i++)
                        {
                            var jobId = requests[indices[i]].Job.Id;
                            if (!pendingIds.Contains(jobId)) continue;
                            //  仍为期望值的行逐行 CAS；此时被并发抢走视为已领取（对方事务里已写了执行行）
                            claimed[i] = await TryClaimOneAsync(jobRepository, requests[indices[i]], cancellationToken);
                        }
                    }
                }

                var claimedAt = _clock.Now;
                for (var i = 0; i < indices.Count; i++)
                {
                    if (!claimed[i]) continue;
                    var request = requests[indices[i]];
                    var executions = new List<SchedulerExecution>(request.Periods.Count);
                    foreach (var period in request.Periods) executions.Add(BuildExecution(request.Job, period, request.Attempt, claimedAt));
                    results[indices[i]] = new ClaimBatchResult { Request = request, Executions = executions };
                    pendingExecutions.AddRange(executions);
                }
            }

            //  写入前剔除已存在的执行键：唯一键冲突会让整批回滚，一条重复就废掉整批 50 条。
            //  已存在说明该周期已被领过（并发重投），对应请求按未领取返回，调用方剔除内存条目即可，
            //  这样单条重复最多丢它自己，不会连带同批其余行。
            if (pendingExecutions.Count > 0)
            {
                var executionRepository = unitOfWork.GetRepository<SchedulerExecution, long>();
                var keys = new List<string>(pendingExecutions.Count);
                foreach (var execution in pendingExecutions) keys.Add(execution.ExecutionKey);

                var existing = await executionRepository.SelectAsync<string>(x => keys.Contains(x.ExecutionKey), x => x.ExecutionKey, null, cancellationToken);
                if (existing is { Count: > 0 })
                {
                    var existingKeys = new HashSet<string>(existing, StringComparer.Ordinal);
                    _logger.LogWarning("领取发现 {Count} 条执行键已存在（并发重投），仅剔除这些条目，其余照常提交", existingKeys.Count);

                    for (var index = 0; index < results.Count; index++)
                    {
                        var executions = results[index].Executions;
                        if (executions != null && executions.Exists(x => existingKeys.Contains(x.ExecutionKey)))
                            results[index].Executions = null;
                    }

                    pendingExecutions.RemoveAll(x => existingKeys.Contains(x.ExecutionKey));
                }
            }

            //  一次批量 INSERT + 一次 COMMIT
            if (pendingExecutions.Count > 0)
                await unitOfWork.GetRepository<SchedulerExecution, long>().InsertAsync(pendingExecutions, cancellationToken);

            await unitOfWork.CommitAsync(cancellationToken);
            return results;
        }
        catch (Exception ex)
        {
            //  事务级异常（唯一键冲突 / 连接中断）：整批回滚，全部按「未领取」处理，下一次扫描会重新校正锚点
            _logger.LogWarning(ex, "批量领取失败，整批 {Count} 条按未领取处理", requests.Count);
            try
            {
                await unitOfWork.RollbackAsync(cancellationToken);
            }
            catch (Exception rollbackException)
            {
                _logger.LogDebug(rollbackException, "批量领取回滚异常");
            }

            var failed = new List<ClaimBatchResult>(requests.Count);
            foreach (var request in requests) failed.Add(new ClaimBatchResult { Request = request, Executions = null });
            return failed;
        }
    }


    /// <summary>
    ///     单行 CAS 领取（条件与单条领取路径保持一致）
    /// </summary>
    private static async Task<bool> TryClaimOneAsync(IRepository<ScheduledJob, long> jobRepository, ClaimBatchRequest request, CancellationToken cancellationToken)
    {
        int affected;
        if (request.Kind == DispatchKind.Retry)
        {
            affected = await jobRepository.UpdateColumnsAsync(x => new ScheduledJob { RetryAtTime = null }
                , x => x.Id == request.Job.Id && x.RetryAtTime == request.ExpectedTime && x.Status == JobStatus.Normal, cancellationToken);
        }
        else
        {
            affected = await jobRepository.UpdateColumnsAsync(x => new ScheduledJob { NextRunTime = request.NewNextRunTime, LastRunTime = request.LastRunTime }
                , x => x.Id == request.Job.Id && x.NextRunTime == request.ExpectedTime && x.Status == JobStatus.Normal, cancellationToken);
        }

        return affected > 0;
    }

    /// <summary>
    ///     多行 CAS 领取：一条 UPDATE 覆盖同组所有任务，返回实际命中行数
    /// </summary>
    private static async Task<int> TryClaimManyAsync(IRepository<ScheduledJob, long> jobRepository, ClaimBatchRequest sample, List<long> ids, CancellationToken cancellationToken)
    {
        if (sample.Kind == DispatchKind.Retry)
        {
            return await jobRepository.UpdateColumnsAsync(x => new ScheduledJob { RetryAtTime = null }
                , x => ids.Contains(x.Id) && x.RetryAtTime == sample.ExpectedTime && x.Status == JobStatus.Normal, cancellationToken);
        }

        return await jobRepository.UpdateColumnsAsync(x => new ScheduledJob { NextRunTime = sample.NewNextRunTime, LastRunTime = sample.LastRunTime }
            , x => ids.Contains(x.Id) && x.NextRunTime == sample.ExpectedTime && x.Status == JobStatus.Normal, cancellationToken);
    }


    /// <summary>
    ///     回查一组任务里「锚点仍等于期望值」的行（即多行 UPDATE 没覆盖到的部分）
    /// </summary>
    private static async Task<HashSet<long>> SelectPendingIdsAsync(IRepository<ScheduledJob, long> jobRepository, ClaimBatchRequest sample, List<long> ids, CancellationToken cancellationToken)
    {
        var pending = sample.Kind == DispatchKind.Retry
            ? await jobRepository.SelectAsync<long>(x => ids.Contains(x.Id) && x.RetryAtTime == sample.ExpectedTime && x.Status == JobStatus.Normal,
                x => x.Id, null, cancellationToken)
            : await jobRepository.SelectAsync<long>(x => ids.Contains(x.Id) && x.NextRunTime == sample.ExpectedTime && x.Status == JobStatus.Normal,
                x => x.Id, null, cancellationToken);

        return pending.Count == 0 ? new HashSet<long>() : new HashSet<long>(pending);
    }

    /// <summary>
    ///     重试领取的单条包装（批量路径里只有一条时复用）
    /// </summary>
    private async Task<List<SchedulerExecution>> ClaimRetryAsListAsync(ClaimBatchRequest request, CancellationToken cancellationToken)
    {
        var execution = await TryClaimRetryAsync(request.Job, request.Periods[0], request.Attempt, request.ExpectedTime, cancellationToken);
        return execution == null ? null : [execution];
    }

    /// <summary>
    ///     领取周期触发点（任务推进 + 执行行，同一事务）
    /// </summary>
    /// <param name="job"></param>
    /// <param name="periods"></param>
    /// <param name="attempt"></param>
    /// <param name="expectedNextRunTime"></param>
    /// <param name="newNextRunTime"></param>
    /// <param name="lastRunTime"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<SchedulerExecution>> TryClaimCycleAsync(ScheduledJob job, IReadOnlyList<string> periods, int attempt, DateTime expectedNextRunTime
        , DateTime newNextRunTime, DateTime lastRunTime, CancellationToken cancellationToken = default)
    {
        if (job == null || periods == null || periods.Count == 0) return null;

        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        await using var unitOfWork = await unitOfWorkManager.GetEntityUnitOfWorkAsync<ScheduledJob>(true, cancellationToken);

        try
        {
            var jobRepository = unitOfWork.GetRepository<ScheduledJob, long>();
            var affected = await jobRepository.UpdateColumnsAsync(x => new ScheduledJob { NextRunTime = newNextRunTime, LastRunTime = lastRunTime },
                x => x.Id == job.Id && x.NextRunTime == expectedNextRunTime && x.Status == JobStatus.Normal, cancellationToken);

            if (affected == 0)
            {
                //  周期已被其他节点领取：整体放弃，不写执行行
                await unitOfWork.RollbackAsync(cancellationToken);
                return null;
            }

            var executionRepository = unitOfWork.GetRepository<SchedulerExecution, long>();
            var executions = new List<SchedulerExecution>(periods.Count);
            var now = _clock.Now;

            foreach (var period in periods)
            {
                var execution = BuildExecution(job, period, attempt, now);
                executions.Add(execution);
            }
            await executionRepository.InsertAsync(executions, cancellationToken);

            await unitOfWork.CommitAsync(cancellationToken);
            return executions;
        }
        catch (Exception ex)
        {
            //  唯一键冲突（同周期重复领取）/连接异常：回滚后按"未领到"处理
            _logger.LogWarning(ex, "任务 [{JobName}] 周期领取失败，按未领取处理", job.JobName);
            try
            {
                await unitOfWork.RollbackAsync(cancellationToken);
            }
            catch (Exception rollbackException)
            {
                _logger.LogDebug(rollbackException, "周期领取失败后回滚异常");
            }

            return null;
        }
    }

    /// <summary>
    ///     领取一次重试（仅清 RetryAtTime，不改周期锚点）
    /// </summary>
    /// <param name="job"></param>
    /// <param name="period"></param>
    /// <param name="attempt"></param>
    /// <param name="expectedRetryAtTime"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<SchedulerExecution> TryClaimRetryAsync(ScheduledJob job, string period, int attempt, DateTime expectedRetryAtTime, CancellationToken cancellationToken = default)
    {
        if (job == null || string.IsNullOrWhiteSpace(period)) return null;

        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        await using var unitOfWork = await unitOfWorkManager.GetEntityUnitOfWorkAsync<ScheduledJob>(true, true, cancellationToken);

        try
        {
            var jobRepository = unitOfWork.GetRepository<ScheduledJob, long>();
            var affected = await jobRepository.UpdateColumnsAsync(x => new ScheduledJob { RetryAtTime = null }
                , x => x.Id == job.Id && x.RetryAtTime == expectedRetryAtTime && x.Status == JobStatus.Normal, cancellationToken);

            if (affected == 0)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                return null;
            }

            var execution = BuildExecution(job, period, attempt, _clock.Now);
            await unitOfWork.GetRepository<SchedulerExecution, long>().InsertAsync(execution, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return execution;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "任务 [{JobName}] 重试领取失败，按未领取处理", job.JobName);
            try
            {
                await unitOfWork.RollbackAsync(cancellationToken);
            }
            catch (Exception rollbackException)
            {
                _logger.LogDebug(rollbackException, "重试领取失败后回滚异常");
            }

            return null;
        }
    }

    /// <summary>
    ///     手动触发：只建执行行，不动周期锚点
    /// </summary>
    /// <param name="job"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<SchedulerExecution> OpenManualAsync(ScheduledJob job, CancellationToken cancellationToken = default)
    {
        if (job == null) return null;

        var execution = BuildExecution(job, "M:" + Guid.NewGuid().ToString("N"), 0, _clock.Now);
        await DbAccess.UseAsync<SchedulerExecution, int>(_serviceProvider, repo => repo.InsertAsync(execution, cancellationToken), cancellationToken);
        return execution;
    }

    /// <summary>
    ///     构造执行行（TraceId 与幂等键在此生成）
    /// </summary>
    /// <param name="job"></param>
    /// <param name="period"></param>
    /// <param name="attempt"></param>
    /// <param name="claimAt"></param>
    /// <returns></returns>
    private SchedulerExecution BuildExecution(ScheduledJob job, string period, int attempt, DateTime claimAt)
    {
        return new SchedulerExecution
        {
            //  执行账本 id 也由框架主键生成器赋值：该列没有 AUTO_INCREMENT，留空会让第二条执行行撞主键
            Id = _keyGenerator.Create(),
            TraceId = Guid.NewGuid().ToString("N"),
            ExecutionKey = $"{job.Id}:{period}:{attempt}",
            JobId = job.Id,
            JobName = job.JobName,
            HandlerType = job.HandlerType,
            Parameter = job.Parameter,
            Period = period,
            Attempt = attempt,
            State = ExecutionState.Claimed,
            DispatchAt = claimAt,
            CreatedTime = claimAt
        };
    }
}
