using Findx.Common;
using Findx.Extensions;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Findx.Extensions.Scheduler.Storage;
using Findx.Redis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     调度端结果处理器：Worker 回报成功/失败后更新 Job 状态，失败按指数退避重试，超限写死信。
///     <para>故障转移安全：无在途记录的回调（旧 Master 内存已丢）按 DB 状态落库决策，不再静默丢弃。</para>
/// </summary>
public class JobResultHandler
{
    /// <summary>
    ///     结果幂等标记 key 前缀（同一 TraceId 只处理一次，防 Worker 重发/重复回调重复消耗重试预算）
    /// </summary>
    private const string ResultProcessedKeyPrefix = "findx:scheduler:result:";

    private readonly IOptions<SchedulerOptions> _options;
    private readonly ScheduledJobStorage _storage;
    private readonly InFlightExecutionRegistry _inFlightRegistry;
    private readonly IRedisClientProvider _redisClientProvider;
    private readonly ILogger<JobResultHandler> _logger;

    private IRedisClient _redisClient;

    /// <summary>
    ///     Ctor
    /// </summary>
    public JobResultHandler(IOptions<SchedulerOptions> options, ScheduledJobStorage storage,
        InFlightExecutionRegistry inFlightRegistry, IRedisClientProvider redisClientProvider,
        ILogger<JobResultHandler> logger)
    {
        _options = options;
        _storage = storage;
        _inFlightRegistry = inFlightRegistry;
        _redisClientProvider = redisClientProvider;
        _logger = logger;
    }

    /// <summary>
    ///     处理 Worker 回报的执行结果
    /// </summary>
    public async Task HandleAsync(JobResultReport report, CancellationToken cancellationToken = default)
    {
        if (report == null || string.IsNullOrWhiteSpace(report.TraceId)) return;

        // 幂等守卫：同一 TraceId 只处理一次（SETNX，TTL 10 分钟覆盖重试/回调时间窗）
        if (!await MarkResultProcessedAsync(report.TraceId, cancellationToken))
        {
            _logger.LogDebug("TraceId={TraceId} 的结果回报已处理过，跳过重复回调", report.TraceId);
            return;
        }

        if (!_inFlightRegistry.TryRemove(report.TraceId, out var flight))
        {
            // 在途记录不存在：可能 Master 刚发生故障转移，旧 Master 内存状态已丢。
            // 不再直接忽略——按 DB 当前状态落库决策（成功幂等落库；失败按 TryCount 推进重试或进死信）
            _logger.LogWarning("收到 TraceId={TraceId} 的结果回报，但无对应在途记录（可能 Master 已切换），按 DB 状态恢复决策", report.TraceId);
            if (report.IsSuccess)
            {
                await _storage.ResetSuccessAsync(report.JobId, cancellationToken);
                _logger.LogInformation("任务 [{JobName}] 孤儿成功回报已幂等落库（成功状态恢复）", report.JobName);
                return;
            }

            await RecoverOrphanFailureAsync(report, cancellationToken);
            return;
        }

        // 释放执行权锁
        if (flight.ExecutionLock != null) await flight.ExecutionLock.DisposeAsync();

        if (report.IsSuccess)
        {
            await _storage.ResetSuccessAsync(flight.JobId, cancellationToken);
            _logger.LogInformation("任务 [{JobName}] TraceId={TraceId} 执行成功，耗时 {Elapsed}ms", flight.JobName, report.TraceId, report.ElapsedMs);
            return;
        }

        await HandleFailureAsync(flight, report.Error ?? report.Message ?? "unknown error", cancellationToken);
    }

    /// <summary>
    ///     统一失败处理（Worker 回报失败 / 超时兜底无回调）：指数退避重试或写死信
    /// </summary>
    public async Task HandleFailureAsync(InFlightExecution flight, string error, CancellationToken cancellationToken = default)
    {
        var opts = _options.Value;
        var nextAttempt = flight.Attempt + 1;

        if (flight.ExecutionLock != null)
        {
            await flight.ExecutionLock.DisposeAsync();
            flight.ExecutionLock = null;
        }

        _logger.LogError("任务 [{JobName}] TraceId={TraceId} 第 {Attempt} 次尝试失败（节点 {Worker}）：{Error}",
            flight.JobName, flight.TraceId, nextAttempt, flight.WorkerAddress, error);

        // 超过最大重试次数：写死信表 + 告警日志，并推进下一周期、归零 TryCount
        // （MaxRetryCount 表示允许的重试次数上限，达到该值后仍失败才进死信）
        if (RetryPolicy.IsRetryExhausted(nextAttempt, opts.MaxRetryCount))
        {
            if (opts.DeadLetterEnabled)
            {
                await _storage.WriteDeadLetterAsync(new JobDeadLetter
                {
                    JobId = flight.JobId,
                    JobName = flight.JobName,
                    HandlerType = flight.HandlerType,
                    Parameter = flight.Parameter,
                    CronExpression = flight.CronExpression,
                    TryCount = nextAttempt,
                    LastError = error,
                    WorkerAddress = flight.WorkerAddress,
                    FiredTime = DateTime.Now,
                    TraceId = flight.TraceId
                }, cancellationToken);
            }

            // 无论是否写死信表，都推进 NextRunTime 并归零 TryCount、清空重试标记：
            // 否则 NextRunTime 停留在过去的退避时刻会被下轮扫描立即提前触发，且 TryCount 跨周期累积耗尽重试预算
            await _storage.ApplyDeadLetterAsync(flight.JobId, flight.CronExpression, DateTime.Now, cancellationToken);

            _logger.LogWarning("任务 [{JobName}] 重试次数已达上限 {MaxRetry}，进入死信流程，需人工介入！", flight.JobName, opts.MaxRetryCount);
            return;
        }

        // 未超最大次数：指数退避后重新分发（由后续扫描轮次拾取，天然支持 Master 故障转移续跑）
        // 重试时刻写入 RetryAtTime，不动 NextRunTime——周期锚点不偏移
        // 首次重试退避 = RetryBaseDelayMs（第 1 次重试 1s→2s→4s→8s…，上限 RetryMaxDelayMs）
        var retryRunTime = RetryPolicy.CalculateRetryAt(DateTime.Now, nextAttempt, opts.RetryBaseDelayMs, opts.RetryMaxDelayMs);
        await _storage.ApplyFailureAsync(flight.JobId, nextAttempt, retryRunTime, cancellationToken);
        _logger.LogWarning("任务 [{JobName}] 将在 {Delay}ms 后（{Time:yyyy-MM-dd HH:mm:ss}）第 {RetryNo} 次重试",
            flight.JobName, (retryRunTime - DateTime.Now).TotalMilliseconds, retryRunTime, nextAttempt);
    }

    /// <summary>
    ///     孤儿失败回报恢复：读 DB 当前 TryCount 决定退避重试或进死信（无在途上下文也能续跑）
    /// </summary>
    private async Task RecoverOrphanFailureAsync(JobResultReport report, CancellationToken cancellationToken)
    {
        var opts = _options.Value;
        var job = await _storage.GetAsync(report.JobId, cancellationToken);
        if (job == null)
        {
            _logger.LogWarning("孤儿失败回报：任务 {JobId} 不存在，忽略", report.JobId);
            return;
        }

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
                    FiredTime = DateTime.Now,
                    TraceId = report.TraceId
                }, cancellationToken);
            }

            await _storage.ApplyDeadLetterAsync(job.Id, job.CronExpression, DateTime.Now, cancellationToken);
            _logger.LogWarning("任务 [{JobName}] 孤儿失败回报已达重试上限，进入死信流程，需人工介入！", job.JobName);
            return;
        }

        var retryAt = RetryPolicy.CalculateRetryAt(DateTime.Now, nextAttempt, opts.RetryBaseDelayMs, opts.RetryMaxDelayMs);
        await _storage.ApplyFailureAsync(job.Id, nextAttempt, retryAt, cancellationToken);
        _logger.LogWarning("任务 [{JobName}] 孤儿失败回报（可能 Master 切换），{Delay}ms 后第 {RetryNo} 次重试",
            job.JobName, (retryAt - DateTime.Now).TotalMilliseconds, nextAttempt);
    }

    /// <summary>
    ///     Redis SETNX 幂等标记：首次置位返回 true；已存在返回 false（该 TraceId 已处理过）
    /// </summary>
    private async Task<bool> MarkResultProcessedAsync(string traceId, CancellationToken cancellationToken)
    {
        try
        {
            _redisClient ??= _redisClientProvider.CreateClient();
            return await _redisClient.StringSetAsync(ResultProcessedKeyPrefix + traceId, "1",
                TimeSpan.FromMinutes(10), When.NotExists, cancellationToken);
        }
        catch (Exception ex)
        {
            // Redis 不可用时降级为不幂等（主流程仍可用；极端重复回调可能重复计数，但不会丢结果）
            _logger.LogWarning(ex, "结果幂等标记写入失败（Redis 不可用），降级处理");
            return true;
        }
    }
}
