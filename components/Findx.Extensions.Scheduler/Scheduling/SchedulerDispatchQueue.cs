using System.Threading.Channels;
using Findx.Extensions.Scheduler.Options;
using Findx.Extensions.Scheduler.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     Master 分发队列：调度循环只做「查到期 + 抢占 + 入队」（纯内存操作），
///     Redis 取执行权锁、Worker HTTP 投递、失败入库全部由独立消费线程执行。
///     <para>目的：让到期扫描节奏不被单次网络往返/Worker 假死拖偏，任务越多、链路越慢越明显。</para>
/// </summary>
public class SchedulerDispatchQueue : IHostedService, IDisposable
{
    /// <summary>
    ///     队列饱和时调度循环等待多久后降级同步分发（毫秒）
    /// </summary>
    private const int QueueFullWaitMs = 500;

    private readonly Channel<DispatchItem> _channel;
    private readonly List<Task> _consumers = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly JobDispatcher _dispatcher;
    private readonly JobResultHandler _resultHandler;
    private readonly ScheduledJobStorage _storage;
    private readonly SchedulerJobBuffer _buffer;
    private readonly ILogger<SchedulerDispatchQueue> _logger;
    private readonly int _workerCount;

    /// <summary>
    ///     Ctor
    /// </summary>
    public SchedulerDispatchQueue(IOptions<SchedulerOptions> options, JobDispatcher dispatcher,
        JobResultHandler resultHandler, ScheduledJobStorage storage, SchedulerJobBuffer buffer,
        ILogger<SchedulerDispatchQueue> logger)
    {
        _dispatcher = dispatcher;
        _resultHandler = resultHandler;
        _storage = storage;
        _buffer = buffer;
        _logger = logger;
        _workerCount = Math.Max(1, options.Value.DispatchWorkerCount);

        var capacity = Math.Max(64, options.Value.DispatchQueueCapacity);
        //  有界通道：写满即背压，避免 Worker 集体变慢时把到期任务无限堆在内存里
        _channel = Channel.CreateBounded<DispatchItem>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = true
        });
    }

    /// <summary>
    ///     尝试入队。返回 false 表示队列已饱和（调用方降级为同步分发，任务不丢弃）
    /// </summary>
    public bool TryEnqueue(DispatchItem item) => _channel.Writer.TryWrite(item);

    /// <summary>
    ///     启动分发消费线程
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var reader = _channel.Reader;
        _consumers.Clear();
        for (var i = 0; i < _workerCount; i++)
        {
            _consumers.Add(Task.Run(() => ConsumeAsync(reader, _cts.Token), CancellationToken.None));
        }

        _logger.LogInformation("任务分发队列已启动，消费线程数={Count}", _workerCount);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     停止消费线程
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        _channel.Writer.TryComplete();
        try
        {
            await Task.WhenAll(_consumers);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "等待任务分发消费线程退出时发生异常");
        }
    }

    /// <summary>
    ///     释放
    /// </summary>
    public void Dispose()
    {
        try
        {
            _cts.Cancel();
            _channel.Writer.TryComplete();
        }
        catch (ObjectDisposedException)
        {
            //  已释放或已停止，忽略
        }
        _cts.Dispose();
    }

    /// <summary>
    ///     CAS 抢占当前周期：成功后该周期由本实例负责投递；失败表示已被其他实例处理（不重复分发）
    /// </summary>
    private Task<bool> ClaimAsync(DispatchItem item, CancellationToken cancellationToken)
    {
        return item.Kind == DispatchKind.Retry
            ? _storage.ClaimRetryAsync(item.Job.Id, item.ExpectedTime, cancellationToken)
            : _storage.ClaimDueAsync(item.Job.Id, item.ExpectedTime, item.NewNextRunTime, item.LastRunTime, cancellationToken);
    }

    /// <summary>
    ///     消费循环：投递到 Worker；投递失败走重试/死信流程，不再回压调度线程
    /// </summary>
    private async Task ConsumeAsync(ChannelReader<DispatchItem> reader, CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var item in reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    //  先 CAS 抢占再投递：抢占失败说明该周期已被其他实例处理，剔除内存条目让后续重查校正
                    if (!await ClaimAsync(item, cancellationToken))
                    {
                        _buffer.Remove(item.Job.Id);
                        continue;
                    }

                    //  一个条目可能携带多个补触发周期（CatchUpWithinWindow），逐个投递，失败即停
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
                        //  分发失败（无可用节点 / Worker 不可达 / 执行权锁被占用）按失败路径退避重试，避免本轮任务直接丢失
                        await _resultHandler.HandleFailureAsync(item.ToFailedFlight(),
                            "分发失败：无可执行节点或 Worker 不可达", cancellationToken);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    //  抢占或投递异常：条目已从内存推进中移除，交由下轮重查恢复真实状态
                    _buffer.Remove(item.Job?.Id ?? 0);
                    _logger.LogError(ex, "任务 [{JobName}] 周期 {Period} 分发异常", item.Job?.JobName, item.Period);
                }
            }
        }
        catch (OperationCanceledException)
        {
            //  停止时正常退出
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "任务分发消费循环异常退出");
        }
    }
}
