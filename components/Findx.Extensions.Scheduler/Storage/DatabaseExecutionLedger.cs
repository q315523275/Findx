using Findx.Data;
using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Findx.Extensions.Scheduler.Storage;

/// <summary>
///     基于数据表的执行账本。
///     <para>状态迁移一律用"带上当前状态谓词的 UPDATE"，影响行数即仲裁结果：
///     0 行说明已被别的节点/回调先一步处理，本次直接放弃，不重试、不覆盖。</para>
/// </summary>
public class DatabaseExecutionLedger : IExecutionLedger
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseExecutionLedger> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public DatabaseExecutionLedger(IServiceProvider serviceProvider, ILogger<DatabaseExecutionLedger> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    ///     按 TraceId 查询
    /// </summary>
    /// <param name="traceId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<SchedulerExecution> GetByTraceAsync(string traceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(traceId)) return null;

        return await DbAccess.UseAsync<SchedulerExecution, SchedulerExecution>(_serviceProvider, repo => repo.FirstAsync(x => x.TraceId == traceId, cancellationToken), cancellationToken);
    }

    /// <summary>
    ///     Claimed -> Dispatched
    /// </summary>
    /// <param name="traceId"></param>
    /// <param name="workerAddress"></param>
    /// <param name="dispatchedAt"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> MarkDispatchedAsync(string traceId, string workerAddress, DateTime dispatchedAt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(traceId)) return false;

        var affected = await DbAccess.UseAsync<SchedulerExecution, int>(_serviceProvider,
            repo => repo.UpdateColumnsAsync(
                x => new SchedulerExecution { State = ExecutionState.Dispatched, WorkerAddress = workerAddress, DispatchAt = dispatchedAt },
                x => x.TraceId == traceId && x.State == ExecutionState.Claimed, cancellationToken), cancellationToken);

        return affected > 0;
    }

    /// <summary>
    ///     Claimed -> Dispatched（批量：一次 UPDATE 覆盖同批已受理的行）
    ///     <para>与逐条版本使用同一个状态谓词，因此部分行已被超时兜底接管时只影响这些行，其余照常推进。</para>
    /// </summary>
    /// <param name="traceIds"></param>
    /// <param name="workerAddress"></param>
    /// <param name="dispatchedAt"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<int> MarkDispatchedBatchAsync(IReadOnlyList<string> traceIds, string workerAddress, DateTime dispatchedAt, CancellationToken cancellationToken = default)
    {
        if (traceIds == null || traceIds.Count == 0) return 0;

        var ids = traceIds.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
        if (ids.Count == 0) return 0;

        //  多行 UPDATE 在多个分发线程并发时，交叉更新同一批主键可能触发 InnoDB 死锁（1213）。
        //  死锁是瞬时冲突，重试即可；不重试会让「已受理但记账失败」的行被误判为未受理并立即重投，造成重复执行。
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return await DbAccess.UseAsync<SchedulerExecution, int>(_serviceProvider,
                    repo => repo.UpdateColumnsAsync(
                        x => new SchedulerExecution { State = ExecutionState.Dispatched, WorkerAddress = workerAddress, DispatchAt = dispatchedAt },
                        x => ids.Contains(x.TraceId) && x.State == ExecutionState.Claimed, cancellationToken), cancellationToken);
            }
            catch (Exception ex) when (attempt < 2 && IsDeadlock(ex))
            {
                _logger.LogWarning("批量受理确认遇到死锁（{Count} 行），第 {Attempt} 次重试", ids.Count, attempt + 1);
                await Task.Delay(20 * (attempt + 1), cancellationToken);
            }
        }
    }

    /// <summary>
    ///     是否 InnoDB 死锁（1213）
    /// </summary>
    private static bool IsDeadlock(Exception ex)
    {
        for (var current = ex; current != null; current = current.InnerException)
        {
            if (current.Message != null && current.Message.Contains("Deadlock", StringComparison.OrdinalIgnoreCase)) return true;
        }

        return false;
    }

    /// <summary>
    ///     终结执行（幂等：重复回调只有一次返回 true）
    /// </summary>
    /// <param name="traceId"></param>
    /// <param name="state"></param>
    /// <param name="error"></param>
    /// <param name="finishedAt"></param>
    /// <param name="elapsedMs"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<bool> TryFinalizeAsync(string traceId, ExecutionState state, string error, DateTime finishedAt, long? elapsedMs, CancellationToken cancellationToken = default)
    {
        return TryFinalizeAsync(traceId, state, error, finishedAt, elapsedMs, null, null, cancellationToken);
    }

    /// <summary>
    ///     终结执行并写入结果消息与执行日志
    /// </summary>
    /// <param name="traceId"></param>
    /// <param name="state"></param>
    /// <param name="error"></param>
    /// <param name="finishedAt"></param>
    /// <param name="elapsedMs"></param>
    /// <param name="message"></param>
    /// <param name="logs"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> TryFinalizeAsync(string traceId, ExecutionState state, string error, DateTime finishedAt, long? elapsedMs, string message, string logs
        , CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(traceId)) return false;
        if (!ExecutionStateHelper.IsTerminal(state))
        {
            _logger.LogWarning("非法终结状态 {State}（TraceId={TraceId}），拒绝写入", state, traceId);
            return false;
        }

        var affected = await DbAccess.UseAsync<SchedulerExecution, int>(_serviceProvider,
            repo => repo.UpdateColumnsAsync(
                x => new SchedulerExecution { State = state, Error = error, Message = message, JobLog = logs, FinishAt = finishedAt, ElapsedMs = elapsedMs },
                x => x.TraceId == traceId && (x.State == ExecutionState.Claimed || x.State == ExecutionState.Dispatched), cancellationToken), cancellationToken);

        return affected > 0;
    }

    /// <summary>
    ///     查询未终结且分发时间早于阈值的执行行
    /// </summary>
    /// <param name="before"></param>
    /// <param name="max"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IReadOnlyList<SchedulerExecution>> ListStaleAsync(DateTime before, int max, CancellationToken cancellationToken = default)
    {
        var rows = await DbAccess.UseAsync<SchedulerExecution, List<SchedulerExecution>>(_serviceProvider,
            repo => repo.TopAsync(Math.Max(1, max),
                x => x.DispatchAt != null && x.DispatchAt < before && (x.State == ExecutionState.Claimed || x.State == ExecutionState.Dispatched),
                [new SortCondition<SchedulerExecution>(nameof(SchedulerExecution.DispatchAt))],
                cancellationToken), cancellationToken);

        return rows;
    }

    /// <summary>
    ///     查询某任务的最近执行记录
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="max"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IReadOnlyList<SchedulerExecution>> ListByJobAsync(long jobId, int max, CancellationToken cancellationToken = default)
    {
        var rows = await DbAccess.UseAsync<SchedulerExecution, List<SchedulerExecution>>(_serviceProvider,
            repo => repo.TopAsync(Math.Max(1, max), x => x.JobId == jobId,
                [new SortCondition<SchedulerExecution>(nameof(SchedulerExecution.Id), ListSortDirection.Descending)], cancellationToken), cancellationToken);

        return rows;
    }

    /// <summary>
    ///     查询已终结且结束时间早于阈值的执行行已终结且早于指定时间的执行行
    /// </summary>
    /// <param name="before"></param>
    /// <param name="max"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IReadOnlyList<SchedulerExecution>> ListTerminalBeforeAsync(DateTime before, int max, CancellationToken cancellationToken = default)
    {
        return await DbAccess.UseAsync<SchedulerExecution, List<SchedulerExecution>>(_serviceProvider,
            repo => repo.TopAsync(Math.Clamp(max, 1, 2000),
                x => x.FinishAt != null && x.FinishAt < before && (x.State == ExecutionState.Succeeded || x.State == ExecutionState.Failed || x.State == ExecutionState.Cancelled),
                [new SortCondition<SchedulerExecution>(nameof(SchedulerExecution.Id))], cancellationToken), cancellationToken);
    }

    /// <summary>
    ///     按 TraceId 批量删除
    /// </summary>
    /// <param name="traceIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<int> DeleteByTraceIdsAsync(IReadOnlyList<string> traceIds, CancellationToken cancellationToken = default)
    {
        if (traceIds == null || traceIds.Count == 0) return 0;

        var ids = traceIds.ToList();
        return await DbAccess.UseAsync<SchedulerExecution, int>(_serviceProvider,
            repo => repo.DeleteAsync(x => ids.Contains(x.TraceId), cancellationToken), cancellationToken);
    }
}
