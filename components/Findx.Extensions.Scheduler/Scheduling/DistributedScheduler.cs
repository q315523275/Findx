using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     Master 统一调度循环：仅 Master 角色运行——扫描内存窗口里到期的任务并入队分发。
///     <para>本循环只做「内存比对 + 入队」：事务领取、HTTP 投递、失败退避在分发队列线程执行；
///     超时兜底是独立节拍的批处理（<see cref="TimeoutSweepService" />）；窗口刷新在后台线程执行。
///     因此单轮耗时基本恒定，不会因为 Worker 变慢或超时集中爆发把 cron 节奏顶偏。</para>
/// </summary>
public class DistributedScheduler : BackgroundService
{
    private readonly IOptions<SchedulerOptions> _options;
    private readonly MasterElectionService _electionService;
    private readonly SchedulerJobBuffer _buffer;
    private readonly SchedulerDispatchQueue _dispatchQueue;
    private readonly JobResultHandler _resultHandler;
    private readonly IClusterClock _clock;
    private readonly ILogger<DistributedScheduler> _logger;

    /// <summary>
    ///     慢循环告警节流（避免每轮都刷日志）
    /// </summary>
    private DateTime _lastSlowWarnAt = DateTime.MinValue;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="electionService"></param>
    /// <param name="buffer"></param>
    /// <param name="dispatchQueue"></param>
    /// <param name="resultHandler"></param>
    /// <param name="clock"></param>
    /// <param name="logger"></param>
    public DistributedScheduler(IOptions<SchedulerOptions> options, MasterElectionService electionService,
        SchedulerJobBuffer buffer, SchedulerDispatchQueue dispatchQueue, JobResultHandler resultHandler,
        IClusterClock clock, ILogger<DistributedScheduler> logger)
    {
        _options = options;
        _electionService = electionService;
        _buffer = buffer;
        _dispatchQueue = dispatchQueue;
        _resultHandler = resultHandler;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    ///     主循环
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opts = _options.Value;
        await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);

        //  扫描节拍按「时间栅格」对齐（epoch 起的整间隔边界），不再用「本轮耗时补偿」的浮动相位：
        //  浮动相位会让 */10s 这类 cron 在 :00 到期、却拖到 :02~:07 才被扫到，精度损失等于一个扫描间隔。
        var intervalTicks = TimeSpan.TicksPerSecond * Math.Max(1, opts.SchedulerScanIntervalSeconds);
        var intervalMs = intervalTicks / TimeSpan.TicksPerMillisecond;

        //  下一个栅格点：当前时间之后最近的对齐边界
        var nextTickTicks = (_clock.Now.Ticks / intervalTicks + 1) * intervalTicks;

        while (!stoppingToken.IsCancellationRequested)
        {
            var waitTicks = nextTickTicks - _clock.Now.Ticks;
            if (waitTicks > 0)
            {
                await Task.Delay(TimeSpan.FromTicks(waitTicks), stoppingToken);

                //  Task.Delay 可能提前几毫秒返回；差几毫秒就会判定「未到期」而整整错过一个栅格，
                //  因此这里补一个到点自旋（通常在 10ms 内结束）。
                while (!stoppingToken.IsCancellationRequested && _clock.Now.Ticks < nextTickTicks)
                    await Task.Delay(1, stoppingToken);
            }

            //  本轮扫描的时间基准 = 栅格点本身，而不是「醒来时的真实时钟」：
            //  否则真实时钟差几毫秒就会把「锚点正好等于栅格点」的任务判成未到期，整波延后一个间隔（500 任务时表现为每隔一波晚 5s）。
            var tickTime = new DateTime(nextTickTicks, DateTimeKind.Local);

            //  栅格只向前推进一格；若上一轮超时导致已越过多个格子，直接跳到未来最近的边界，不补跑
            nextTickTicks += intervalTicks;
            var nowTicks = _clock.Now.Ticks;
            if (nextTickTicks <= nowTicks)
                nextTickTicks = (nowTicks / intervalTicks + 1) * intervalTicks;

            var startTime = _clock.Now;
            try
            {
                //  只有 Master 统一调度；Standby 空转等待选举
                if (opts.SchedulerEnabled && _electionService.IsMaster)
                    await ScanAndDispatchAsync(tickTime, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调度循环异常");
            }

            var costMs = (_clock.Now - startTime).TotalMilliseconds;

            //  单轮耗时超过半周期：说明有阻塞点回来了（窗口同步加载 / 降级分发等），需要告警而不是静默漂移
            if (costMs > intervalMs * 0.5 && (DateTime.Now - _lastSlowWarnAt).TotalSeconds >= 30)
            {
                _lastSlowWarnAt = DateTime.Now;
                _logger.LogWarning("调度循环单轮耗时 {Cost}ms，已超过扫描间隔 {Interval}ms 的一半，到期任务可能延后投递", (int)costMs, intervalMs);
            }
        }
    }

    /// <summary>
    ///     扫描到期任务（内存窗口）：重试到期优先，其次周期到期。
    ///     <para>正常情况下本轮只有一次内存快照与排序；窗口刷新由 SchedulerJobBuffer 在后台完成，
    ///     调度线程不做数据库往返与网络调用。唯一例外是 Master 当选瞬间窗口尚未预热（有 3 秒启动延迟兜底）。</para>
    /// </summary>
    ///     <param name="scanTime">本轮栅格点时间（到期判定基准，避免真实时钟毫秒抖动导致整波延后）</param>
    /// <param name="cancellationToken"></param>
    private async Task ScanAndDispatchAsync(DateTime scanTime, CancellationToken cancellationToken)
    {
        var dueItems = await _buffer.ScanDueAsync(scanTime, cancellationToken);
        if (dueItems.Count == 0) return;

        _logger.LogDebug("栅格 {Tick:HH:mm:ss.fff} 扫描到 {Count} 个到期任务", scanTime, dueItems.Count);

        foreach (var item in dueItems)
        {
            if (cancellationToken.IsCancellationRequested) break;
            await EnqueueOrDispatchAsync(item, cancellationToken);
        }
    }

    /// <summary>
    ///     入队分发。
    ///     <para>队列饱和（Worker 集体变慢、投递积压）时不再在调度线程里同步领取 + HTTP 投递：
    ///     那会在最忙的时候把扫描周期整段吃掉。改为「本周期延后、下轮重试」——不消耗重试次数，
    ///     也不产生执行流水，语义与「暂无可用节点」一致。</para>
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    private async Task EnqueueOrDispatchAsync(DispatchItem item, CancellationToken cancellationToken)
    {
        if (_dispatchQueue.TryEnqueue(item)) return;

        _logger.LogWarning("任务 [{JobName}] 分发队列已饱和，本周期延后 {Seconds}s 后再试（不消耗重试次数）", item.Job.JobName, Math.Max(5, _options.Value.SchedulerScanIntervalSeconds));

        await _resultHandler.DeferNoWorkerAsync(item, "分发队列饱和，本周期延后", cancellationToken);
    }
}
