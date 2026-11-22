using System.Threading.Channels;
using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Execution;

/// <summary>
///     执行日志发送队列：执行过程中产生的日志分片先入有界队列，后台按批次 HTTP 上报调度端。
///     <para>为什么要走队列：一次执行可能产生上千行日志，既不能塞进结果回调（体积/延迟都不可控），
///     也不能在执行线程里同步等待 HTTP（会拖慢任务并放大超时风险）。队列 + 批量上报把网络抖动挡在执行路径之外；
///     队列满时丢弃最旧日志并计数，保证任务本身不被日志拖垮。</para>
/// </summary>
public sealed class ExecutionLogShipper : IHostedService, IDisposable
{
    private readonly IOptions<SchedulerOptions> _options;
    private readonly ISchedulerMasterChannel _masterChannel;
    private readonly ILogger<ExecutionLogShipper> _logger;
    private readonly Channel<ExecutionLogChunk> _channel;
    private readonly CancellationTokenSource _cts = new();

    private Task _consumer;
    private long _dropped;
    private long _failedBatches;
    private long _postedChunks;

    /// <summary>
    ///     Ctor
    /// </summary>
    public ExecutionLogShipper(IOptions<SchedulerOptions> options, ISchedulerMasterChannel channel,
        ILogger<ExecutionLogShipper> logger)
    {
        _options = options;
        _masterChannel = channel;
        _logger = logger;

        var capacity = Math.Max(64, options.Value.LogShipQueueCapacity);
        _channel = Channel.CreateBounded<ExecutionLogChunk>(new BoundedChannelOptions(capacity)
        {
            SingleReader = true,
            SingleWriter = false,
            //  满时丢最旧：日志是旁证，不能反压执行
            FullMode = BoundedChannelFullMode.DropOldest
        });
    }

    /// <summary>
    ///     入队一片日志（非阻塞）
    /// </summary>
    public void Ship(ExecutionLogChunk chunk)
    {
        if (chunk == null || string.IsNullOrEmpty(chunk.Content)) return;
        if (!_channel.Writer.TryWrite(chunk)) Interlocked.Increment(ref _dropped);
    }

    /// <summary>
    ///     启动后台消费者
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _consumer = Task.Run(() => ConsumeAsync(_cts.Token), CancellationToken.None);
        _logger.LogInformation("执行日志上报队列已启动，容量={Capacity}", _options.Value.LogShipQueueCapacity);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     停止：把队列里剩下的分片尽量上报完
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _channel.Writer.TryComplete();
        try
        {
            if (_consumer != null)
            {
                await Task.WhenAny(_consumer, Task.Delay(TimeSpan.FromSeconds(3), cancellationToken));
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "执行日志上报队列停止时异常");
        }

        _cts.Cancel();
    }

    /// <summary>
    ///     释放
    /// </summary>
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    /// <summary>
    ///     消费循环：攒批 -> 上报
    /// </summary>
    private async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        var reader = _channel.Reader;
        var batchSize = Math.Max(1, _options.Value.LogShipBatchSize);
        var intervalMs = Math.Max(200, _options.Value.LogShipIntervalMs);

        try
        {
            while (await reader.WaitToReadAsync(cancellationToken))
            {
                var batch = new List<ExecutionLogChunk>(batchSize);
                while (batch.Count < batchSize && reader.TryRead(out var chunk)) batch.Add(chunk);
                if (batch.Count == 0) continue;

                //  再等一个发送窗口，把同一次执行的连续分片合并成尽量少的 HTTP 请求
                var deadline = DateTime.UtcNow.AddMilliseconds(intervalMs);
                while (batch.Count < batchSize && DateTime.UtcNow < deadline && !cancellationToken.IsCancellationRequested)
                {
                    if (reader.TryRead(out var chunk))
                    {
                        batch.Add(chunk);
                        continue;
                    }

                    await Task.Delay(50, cancellationToken);
                }

                await PostBatchAsync(batch, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            //  正常停止
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行日志上报消费循环异常退出");
        }
    }

    /// <summary>
    ///     批量上报：交给协议通道（HTTP 实现内做 2 次短退避重试）；失败只计数，日志是旁证不阻塞执行
    /// </summary>
    private async Task PostBatchAsync(List<ExecutionLogChunk> batch, CancellationToken cancellationToken)
    {
        var address = batch[0].MasterAddress;
        if (string.IsNullOrWhiteSpace(address) && (_options.Value.MasterEndpoints ?? []).Length == 0)
        {
            Interlocked.Add(ref _failedBatches, 1);
            _logger.LogWarning("执行日志上报缺少调度端地址，丢弃 {Count} 片（TraceId={TraceId}）", batch.Count, batch[0].TraceId);
            return;
        }

        if (await _masterChannel.ShipLogsAsync(address, batch, cancellationToken))
        {
            Interlocked.Add(ref _postedChunks, batch.Count);
            return;
        }

        Interlocked.Add(ref _failedBatches, 1);
        _logger.LogWarning("执行日志上报失败，丢弃 {Count} 片（TraceId={TraceId}，协议 {Protocol}，累计失败批次 {Failed}）",
            batch.Count, batch[0].TraceId, _masterChannel.Protocol, Interlocked.Read(ref _failedBatches));
    }
}
