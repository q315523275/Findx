using Findx.Extensions.Scheduler.Storage;
using Findx.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     Master 统一调度循环：仅 Master 角色运行——读持久化 Job 定义、cron 计算触发时刻、到期 CAS 抢占后分发执行，并兜底超时在途任务。
/// </summary>
public class DistributedScheduler : BackgroundService
{
    private readonly IOptions<SchedulerOptions> _options;
    private readonly MasterElectionService _electionService;
    private readonly ScheduledJobStorage _storage;
    private readonly JobDispatcher _dispatcher;
    private readonly JobResultHandler _resultHandler;
    private readonly InFlightExecutionRegistry _inFlightRegistry;
    private readonly ILogger<DistributedScheduler> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public DistributedScheduler(IOptions<SchedulerOptions> options, MasterElectionService electionService,
        ScheduledJobStorage storage, JobDispatcher dispatcher, JobResultHandler resultHandler,
        InFlightExecutionRegistry inFlightRegistry, ILogger<DistributedScheduler> logger)
    {
        _options = options;
        _electionService = electionService;
        _storage = storage;
        _dispatcher = dispatcher;
        _resultHandler = resultHandler;
        _inFlightRegistry = inFlightRegistry;
        _logger = logger;
    }

    /// <summary>
    ///     主循环
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opts = _options.Value;
        await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 只有 Master 统一调度；Standby 空转等待选举
                if (opts.SchedulerEnabled && _electionService.IsMaster)
                {
                    await ScanAndDispatchAsync(stoppingToken);
                    await SweepTimeoutExecutionsAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调度循环异常");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, opts.SchedulerScanIntervalSeconds)), stoppingToken);
        }
    }

    /// <summary>
    ///     扫描到期任务：先处理重试到期（RetryAtTime，CAS 清标记后重发），再处理周期到期
    ///     （CAS 抢占推进 NextRunTime，抢到者负责分发；过期即补触发一次，不补多个错过周期）
    /// </summary>
    private async Task ScanAndDispatchAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var dueJobs = await _storage.GetDueJobsAsync(now, cancellationToken);
        if (dueJobs.Count == 0) return;

        _logger.LogDebug("扫描到 {Count} 个到期任务", dueJobs.Count);

        foreach (var job in dueJobs)
        {
            if (cancellationToken.IsCancellationRequested) break;

            if (string.IsNullOrWhiteSpace(job.CronExpression))
            {
                _logger.LogWarning("任务 [{JobName}] Cron 表达式为空，跳过", job.JobName);
                continue;
            }

            // 分支一：重试到期（上次失败退避到点）——先于周期分支处理，避免重试长期积压
            if (job.RetryAtTime != null && job.RetryAtTime <= now)
            {
                var expectedRetry = job.RetryAtTime.Value;
                var claimedRetry = await _storage.ClaimRetryAsync(job.Id, expectedRetry, cancellationToken);
                if (!claimedRetry)
                {
                    _logger.LogDebug("任务 [{JobName}] 重试时刻已被抢占，跳过", job.JobName);
                    continue;
                }

                // 重试以独立 period 标记（区别于正常周期），执行权锁 key 不冲突
                var retryPeriod = "R:" + expectedRetry.ToString("O");
                var retryDispatched = await _dispatcher.DispatchAsync(job, retryPeriod, job.TryCount, cancellationToken);
                if (!retryDispatched)
                {
                    await _resultHandler.HandleFailureAsync(new InFlightExecution
                    {
                        TraceId = Guid.NewGuid().ToString("N"),
                        JobId = job.Id,
                        JobName = job.JobName,
                        HandlerType = job.HandlerType,
                        Parameter = job.Parameter,
                        Period = retryPeriod,
                        Attempt = job.TryCount,
                        WorkerAddress = string.Empty,
                        StartTime = now,
                        CronExpression = job.CronExpression
                    }, "重试分发失败：无可执行节点或 Worker 不可达", cancellationToken);
                }
                continue;
            }

            // 分支二：周期到期
            DateTime newNext;
            try
            {
                newNext = CronUtility.GetNextOccurrence(job.CronExpression, now);
                if (newNext == default)
                {
                    _logger.LogWarning("任务 [{JobName}] Cron 表达式 {Cron} 无法计算下次触发时间，跳过", job.JobName, job.CronExpression);
                    continue;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "任务 [{JobName}] Cron 表达式 {Cron} 解析失败", job.JobName, job.CronExpression);
                continue;
            }

            // 周期 CAS 抢占：仅当 NextRunTime 仍等于扫描时读到的值才推进，防止并发 Master 重复分发
            var expectedNext = job.NextRunTime!.Value;
            var claimed = await _storage.ClaimDueAsync(job.Id, expectedNext, newNext, now, cancellationToken);
            if (!claimed)
            {
                _logger.LogDebug("任务 [{JobName}] 周期 {Period} 已被抢占，跳过", job.JobName, expectedNext);
                continue;
            }

            var period = expectedNext.ToString("O");
            var dispatched = await _dispatcher.DispatchAsync(job, period, job.TryCount, cancellationToken);
            if (!dispatched)
            {
                // 分发失败（无可用节点/HTTP 异常）按失败路径退避重试，避免本轮任务直接丢失
                await _resultHandler.HandleFailureAsync(new InFlightExecution
                {
                    TraceId = Guid.NewGuid().ToString("N"),
                    JobId = job.Id,
                    JobName = job.JobName,
                    HandlerType = job.HandlerType,
                    Parameter = job.Parameter,
                    Period = period,
                    Attempt = job.TryCount,
                    WorkerAddress = string.Empty,
                    StartTime = now,
                    CronExpression = job.CronExpression
                }, "分发失败：无可执行节点或 Worker 不可达", cancellationToken);
            }
        }
    }

    /// <summary>
    ///     在途执行超时兜底：超过 ExecutionTimeoutMinutes 无回调按失败处理
    /// </summary>
    private async Task SweepTimeoutExecutionsAsync(CancellationToken cancellationToken)
    {
        var opts = _options.Value;
        if (opts.ExecutionTimeoutMinutes <= 0) return;

        var cutoff = DateTime.Now.AddMinutes(-opts.ExecutionTimeoutMinutes);
        foreach (var flight in _inFlightRegistry.GetAll().Where(f => f.StartTime < cutoff).ToList())
        {
            if (!_inFlightRegistry.TryRemove(flight.TraceId, out var timedOut)) continue;
            _logger.LogWarning("任务 [{JobName}] TraceId={TraceId} 执行超过 {Timeout} 分钟无回调，按失败处理",
                flight.JobName, flight.TraceId, opts.ExecutionTimeoutMinutes);
            await _resultHandler.HandleFailureAsync(timedOut, $"执行超时：{opts.ExecutionTimeoutMinutes} 分钟无结果回调", cancellationToken);
        }
    }
}
