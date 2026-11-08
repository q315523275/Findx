using Findx.Extensions.Scheduler.Options;
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
    private readonly SchedulerDispatchQueue _dispatchQueue;
    private readonly SchedulerJobBuffer _buffer;
    private readonly JobResultHandler _resultHandler;
    private readonly InFlightExecutionRegistry _inFlightRegistry;
    private readonly ILogger<DistributedScheduler> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public DistributedScheduler(IOptions<SchedulerOptions> options, MasterElectionService electionService,
        ScheduledJobStorage storage, JobDispatcher dispatcher, SchedulerDispatchQueue dispatchQueue,
        SchedulerJobBuffer buffer, JobResultHandler resultHandler, InFlightExecutionRegistry inFlightRegistry,
        ILogger<DistributedScheduler> logger)
    {
        _options = options;
        _electionService = electionService;
        _storage = storage;
        _dispatcher = dispatcher;
        _dispatchQueue = dispatchQueue;
        _buffer = buffer;
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

        //  周期按毫秒计，每轮用「周期 - 本轮耗时」做补偿，避免扫描耗时累积成调度漂移
        var intervalMs = Math.Max(1, opts.SchedulerScanIntervalSeconds) * 1000;

        while (!stoppingToken.IsCancellationRequested)
        {
            var startTime = DateTime.Now;
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

            var costMs = (DateTime.Now - startTime).TotalMilliseconds;
            var delayMs = intervalMs - costMs;
            if (delayMs > 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(delayMs), stoppingToken);
            }
        }
    }

    /// <summary>
    ///     扫描到期任务（内存窗口）：重试到期优先，其次周期到期。
    ///     <para>窗口查询由 SchedulerJobBuffer 负责（耗尽的刷新在后台进行），CAS 抢占在分发线程执行，
    ///     调度线程本身不做任何数据库往返与网络调用。</para>
    /// </summary>
    private async Task ScanAndDispatchAsync(CancellationToken cancellationToken)
    {
        var dueItems = await _buffer.ScanDueAsync(DateTime.Now, cancellationToken);
        if (dueItems.Count == 0) return;

        _logger.LogDebug("扫描到 {Count} 个到期任务", dueItems.Count);

        foreach (var item in dueItems)
        {
            if (cancellationToken.IsCancellationRequested) break;
            await EnqueueOrDispatchAsync(item, cancellationToken);
        }
    }

    /// <summary>
    ///     入队分发；队列饱和时降级为「本线程 CAS + 同步分发」，保证到期任务不被丢弃
    /// </summary>
    private async Task EnqueueOrDispatchAsync(DispatchItem item, CancellationToken cancellationToken)
    {
        if (_dispatchQueue.TryEnqueue(item)) return;

        _logger.LogWarning("任务 [{JobName}] 分发队列已满，本次降级为同步分发", item.Job.JobName);

        var claimed = item.Kind == DispatchKind.Retry
            ? await _storage.ClaimRetryAsync(item.Job.Id, item.ExpectedTime, cancellationToken)
            : await _storage.ClaimDueAsync(item.Job.Id, item.ExpectedTime, item.NewNextRunTime, item.LastRunTime, cancellationToken);
        if (!claimed)
        {
            _buffer.Remove(item.Job.Id);
            return;
        }

        var allDispatched = true;
        foreach (var period in item.Periods)
        {
            var dispatched = await _dispatcher.DispatchAsync(item.Job, period, item.Attempt, cancellationToken);
            if (dispatched) continue;

            allDispatched = false;
            break;
        }

        if (!allDispatched)
        {
            await _resultHandler.HandleFailureAsync(item.ToFailedFlight(), "分发失败：无可执行节点或 Worker 不可达", cancellationToken);
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
