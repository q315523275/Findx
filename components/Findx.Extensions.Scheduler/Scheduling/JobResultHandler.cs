using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Findx.Extensions.Scheduler.Storage;
using Findx.Jobs.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     调度端结果处理器：根据执行行的状态迁移驱动成功归零 / 指数退避重试 / 死信隔离。
///     <para>幂等来源从 Redis SETNX 改为执行行状态 CAS：重复回调只有一次能把行从 Claimed/Dispatched 终结，
///     没有 TTL 窗口，也不存在"Redis 不可用就退化为不幂等"的路径。</para>
/// </summary>
public class JobResultHandler
{
    /// <summary>
    ///     结果消息/错误落库的最大字符数
    /// </summary>
    private const int MaxResultMessageChars = 5000;

    private readonly IOptions<SchedulerOptions> _options;
    private readonly ScheduledJobStorage _storage;
    private readonly IExecutionLedger _ledger;
    private readonly IClusterClock _clock;
    private readonly ILogger<JobResultHandler> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="storage"></param>
    /// <param name="ledger"></param>
    /// <param name="clock"></param>
    /// <param name="logger"></param>
    public JobResultHandler(IOptions<SchedulerOptions> options, ScheduledJobStorage storage, IExecutionLedger ledger, IClusterClock clock, ILogger<JobResultHandler> logger)
    {
        _options = options;
        _storage = storage;
        _ledger = ledger;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    ///     处理 Worker 回报的执行结果
    /// </summary>
    /// <param name="report"></param>
    /// <param name="cancellationToken"></param>
    public async Task HandleAsync(JobResultReport report, CancellationToken cancellationToken = default)
    {
        if (report == null || string.IsNullOrWhiteSpace(report.TraceId)) return;

        var execution = await _ledger.GetByTraceAsync(report.TraceId, cancellationToken);
        if (execution == null)
        {
            //  执行行不存在：任务可能已被删除，或执行行被运维清理。退化为按任务当前状态处理
            await RecoverOrphanAsync(report, cancellationToken);
            return;
        }

        if (ExecutionStateHelper.IsTerminal(execution.State))
        {
            _logger.LogDebug("TraceId={TraceId} 已是终态 {State}，忽略重复回调", report.TraceId, execution.State);
            return;
        }

        var now = _clock.Now;
        var message = Clamp(report.Message, MaxResultMessageChars);
        var logs = Clamp(report.Logs, _options.Value.MaxExecutionLogChars);

        if (report.IsSuccess)
        {
            if (!await _ledger.TryFinalizeAsync(report.TraceId, ExecutionState.Succeeded, null, now, report.ElapsedMs, message, logs, cancellationToken))
            {
                _logger.LogDebug("TraceId={TraceId} 成功回调未获得终结权（已被其他流程处理），忽略", report.TraceId);
                return;
            }

            await _storage.ResetSuccessAsync(execution.JobId, cancellationToken);
            _logger.LogInformation("任务 [{JobName}] TraceId={TraceId} 执行成功，耗时 {Elapsed}ms", execution.JobName, report.TraceId, report.ElapsedMs);
            return;
        }

        var error = Clamp(report.Error ?? report.Message, MaxResultMessageChars) ?? "unknown error";
        if (!await _ledger.TryFinalizeAsync(report.TraceId, ExecutionState.Failed, error, now, report.ElapsedMs, message, logs, cancellationToken))
        {
            _logger.LogDebug("TraceId={TraceId} 失败回调未获得终结权（已被其他流程处理），忽略", report.TraceId);
            return;
        }

        await HandleFailureAsync(execution, error, cancellationToken);
    }

    /// <summary>
    ///     统一失败处理（回调失败 / 分发失败 / 超时兜底）：指数退避重试，或达到上限后死信隔离
    /// </summary>
    /// <param name="execution"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    public async Task HandleFailureAsync(SchedulerExecution execution, string error, CancellationToken cancellationToken = default)
    {
        if (execution == null) return;

        var opts = _options.Value;
        var nextAttempt = execution.Attempt + 1;

        _logger.LogError("任务 [{JobName}] TraceId={TraceId} 第 {Attempt} 次尝试失败（节点 {Worker}）：{Error}", execution.JobName, execution.TraceId, nextAttempt, execution.WorkerAddress, error);

        if (RetryPolicy.IsRetryExhausted(nextAttempt, opts.MaxRetryCount))
        {
            if (opts.DeadLetterEnabled)
            {
                //  死信记录只在隔离时写一次，补查任务定义以填充 Cron，便于人工放行时核对
                var job = await _storage.GetAsync(execution.JobId, cancellationToken);

                await _storage.WriteDeadLetterAsync(new JobDeadLetter
                {
                    JobId = execution.JobId,
                    JobName = execution.JobName,
                    HandlerType = execution.HandlerType,
                    Parameter = execution.Parameter,
                    CronExpression = job?.CronExpression,
                    TryCount = nextAttempt,
                    LastError = error,
                    WorkerAddress = execution.WorkerAddress,
                    FiredTime = _clock.Now,
                    TraceId = execution.TraceId
                }, cancellationToken);
            }

            //  真正的死信隔离：任务置为 Dead 并清空时间锚点，扫描不再取到，只能人工放行
            var isolated = await _storage.IsolateAsync(execution.JobId, cancellationToken);
            if (isolated > 0)
                _logger.LogWarning("任务 [{JobName}] 重试次数已达上限 {MaxRetry}，已死信隔离，等待人工放行", execution.JobName, opts.MaxRetryCount);
            else
                _logger.LogWarning("任务 [{JobName}] 重试次数已达上限，但隔离未生效（任务可能已删除）", execution.JobName);
            return;
        }

        //  未超上限：写退避时刻，不动周期锚点；扫描下一轮自然拾取
        var retryAt = RetryPolicy.CalculateRetryAt(_clock.Now, nextAttempt, opts.RetryBaseDelayMs, opts.RetryMaxDelayMs);
        var affected = await _storage.ApplyFailureAsync(execution.JobId, nextAttempt, retryAt, cancellationToken);
        if (affected == 0)
        {
            _logger.LogWarning("任务 [{JobName}] 退避时刻写入失败（任务可能已删除或已隔离）", execution.JobName);
            return;
        }

        _logger.LogWarning("任务 [{JobName}] 将在 {Delay}ms 后（{Time:yyyy-MM-dd HH:mm:ss}）第 {RetryNo} 次重试", execution.JobName, (retryAt - _clock.Now).TotalMilliseconds, retryAt, nextAttempt);
    }

    /// <summary>
    ///     文本裁剪：超长保留尾部，避免异常堆栈把账本行撑爆
    /// </summary>
    /// <param name="text"></param>
    /// <param name="maxChars"></param>
    /// <returns></returns>
    private static string Clamp(string text, int maxChars)
    {
        if (string.IsNullOrEmpty(text)) return text;
        if (maxChars <= 0 || text.Length <= maxChars) return text;
        return "…" + text[^maxChars..];
    }

    /// <summary>
    ///     无可用执行节点（领取前预检命中）：不建执行行，只把重试时刻推到下一轮。
    ///     <para>周期条目必须同时推进周期锚点：只写退避时刻会把「该周期仍到期未领取」留在库里，
    ///     等这次后移的重试分发完成后，同一周期又会被周期分支领走，造成同一触发点被执行两次。</para>
    /// </summary>
    /// <param name="item"></param>
    /// <param name="reason"></param>
    /// <param name="cancellationToken"></param>
    public async Task DeferNoWorkerAsync(DispatchItem item, string reason = null, CancellationToken cancellationToken = default)
    {
        if (item?.Job == null) return;

        var job = item.Job;
        var opts = _options.Value;
        var seconds = Math.Max(5, opts.SchedulerScanIntervalSeconds);
        var retryAt = _clock.Now.AddSeconds(seconds);

        if (item.Kind == DispatchKind.Retry)
        {
            //  重试条目：周期锚点已在首次领取时推进过，这里只顺延退避时刻，TryCount 保持原值
            await _storage.ApplyFailureAsync(job.Id, job.TryCount, retryAt, cancellationToken);
            _logger.LogWarning("任务 [{JobName}] {Reason}，重试顺延 {Seconds}s（不消耗重试次数、不产生执行流水）", job.JobName, reason ?? "当前没有可执行节点", seconds);
            return;
        }

        //  周期条目：CAS 推进锚点 + 写退避时刻，一步把「到期未领取」状态清掉
        var affected = await _storage.DeferCycleAsync(job.Id, item.ExpectedTime, item.NewNextRunTime, item.LastRunTime, retryAt, cancellationToken);
        if (affected == 0)
        {
            _logger.LogDebug("任务 [{JobName}] 周期 {Period} 无节点延后时锚点已被其他节点推进，本次放弃", job.JobName, item.Period);
            return;
        }

        _logger.LogWarning("任务 [{JobName}] {Reason}，已跳过本周期并在 {Seconds}s 后重试（不消耗重试次数）", job.JobName, reason ?? "当前没有可执行节点", seconds);
    }

    /// <summary>
    ///     无可用执行节点：不消耗重试预算，只把重试时刻推到下一轮扫描，等节点恢复后继续。
    ///     <para>与"执行失败"区别在于语义：没有任何 Worker 能接单并不是任务的失败，
    ///     若按失败计次，节点短暂下线（滚动重启、扩缩容）就会把任务直接推进死信。</para>
    /// </summary>
    /// <param name="execution"></param>
    /// <param name="cancellationToken"></param>
    public async Task HandleNoWorkerAsync(SchedulerExecution execution, CancellationToken cancellationToken = default)
    {
        if (execution == null) return;

        var opts = _options.Value;
        var retryAt = _clock.Now.AddSeconds(Math.Max(5, opts.SchedulerScanIntervalSeconds));

        //  TryCount 保持原值（传 execution.Attempt 即当前计数），只写重试时刻
        await _storage.ApplyFailureAsync(execution.JobId, execution.Attempt, retryAt, cancellationToken);

        _logger.LogWarning("任务 [{JobName}] 当前没有可执行节点，{Seconds}s 后重新尝试（不消耗重试次数）", execution.JobName, Math.Max(5, opts.SchedulerScanIntervalSeconds));
    }

    /// <summary>
    ///     孤儿回调恢复：执行行已不存在时，按任务当前状态做最后一次决策（成功幂等、失败续跑）
    /// </summary>
    /// <param name="report"></param>
    /// <param name="cancellationToken"></param>
    private async Task RecoverOrphanAsync(JobResultReport report, CancellationToken cancellationToken)
    {
        var job = await _storage.GetAsync(report.JobId, cancellationToken);
        if (job == null)
        {
            _logger.LogWarning("回调 TraceId={TraceId} 无对应执行行且任务 {JobId} 不存在，忽略", report.TraceId, report.JobId);
            return;
        }

        if (job.Status != JobStatus.Normal)
        {
            _logger.LogInformation("回调 TraceId={TraceId} 对应任务 [{JobName}] 当前状态 {Status}，忽略结果", report.TraceId, job.JobName, job.Status);
            return;
        }

        if (report.IsSuccess)
        {
            await _storage.ResetSuccessAsync(job.Id, cancellationToken);
            _logger.LogInformation("孤儿成功回调已幂等落库：任务 [{JobName}]", job.JobName);
            return;
        }

        var opts = _options.Value;
        var nextAttempt = job.TryCount + 1;
        var error = report.Error ?? report.Message ?? "unknown error";

        if (RetryPolicy.IsRetryExhausted(nextAttempt, opts.MaxRetryCount))
        {
            if (opts.DeadLetterEnabled)
            {
                await _storage.WriteDeadLetterAsync(new JobDeadLetter
                {
                    JobId = job.Id,
                    JobName = job.JobName,
                    HandlerType = job.HandlerType,
                    Parameter = job.Parameter,
                    CronExpression = job.CronExpression,
                    TryCount = nextAttempt,
                    LastError = error,
                    WorkerAddress = report.WorkerAddress,
                    FiredTime = _clock.Now,
                    TraceId = report.TraceId
                }, cancellationToken);
            }

            await _storage.IsolateAsync(job.Id, cancellationToken);
            _logger.LogWarning("任务 [{JobName}] 孤儿失败回调已达重试上限，已死信隔离", job.JobName);
            return;
        }

        var retryAt = RetryPolicy.CalculateRetryAt(_clock.Now, nextAttempt, opts.RetryBaseDelayMs, opts.RetryMaxDelayMs);
        await _storage.ApplyFailureAsync(job.Id, nextAttempt, retryAt, cancellationToken);
        _logger.LogWarning("任务 [{JobName}] 孤儿失败回调（执行行缺失），{Delay}ms 后第 {RetryNo} 次重试", job.JobName, (retryAt - _clock.Now).TotalMilliseconds, nextAttempt);
    }
}
