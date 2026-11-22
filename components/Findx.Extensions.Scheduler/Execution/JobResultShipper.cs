using System.Threading.Channels;
using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Execution;

/// <summary>
///     执行结果上报队列：执行结束后结果先入有界队列，后台按窗口合并成批上报调度端。
///     <para>为什么要合批：一条结果一次 HTTP 时，3000 条一波就是 3000 次上行请求，与下发一样成为吞吐上限。
///     合批只改变运输方式，不改语义——调度端仍按 TraceId 逐条幂等终结。</para>
///     <para>队列满时不做丢弃：<see cref="TryShip" /> 返回 false，调用方回落逐条上报，结果不能丢。</para>
/// </summary>
public sealed class JobResultShipper : IHostedService, IDisposable
{
    private readonly IOptions<SchedulerOptions> _options;
    private readonly ISchedulerMasterChannel _masterChannel;
    private readonly ILogger<JobResultShipper> _logger;
    private readonly Channel<ShipItem> _channel;
    private readonly CancellationTokenSource _cts = new();
    private readonly bool _enabled;
    private readonly int _concurrency;
    private SemaphoreSlim _shipGate;

    private Task _consumer;
    private long _failedBatches;
    private long _postedResults;

    /// <summary>
    ///     Ctor
    /// </summary>
    public JobResultShipper(IOptions<SchedulerOptions> options, ISchedulerMasterChannel channel, ILogger<JobResultShipper> logger)
    {
        _options = options;
        _masterChannel = channel;
        _logger = logger;
        //  1 = 关闭批量上报，走旧的逐条回调
        _enabled = options.Value.ResultShipBatchSize > 1;
        _concurrency = Math.Clamp(options.Value.EffectiveResultShipConcurrency, 1, 32);
        _shipGate = new SemaphoreSlim(_concurrency, _concurrency);

        var capacity = Math.Max(64, options.Value.EffectiveResultShipQueueCapacity);
        _channel = Channel.CreateBounded<ShipItem>(new BoundedChannelOptions(capacity)
        {
            SingleReader = true,
            SingleWriter = false,
            //  满时写失败（不覆盖已有结果），由调用方回落逐条上报
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    /// <summary>
    ///     入队一条结果（非阻塞）。返回 false 表示批量通道不可用（已关闭或队列已满），调用方应逐条上报
    /// </summary>
    /// <param name="report"></param>
    /// <param name="masterAddress">分发包携带的调度端地址</param>
    /// <returns></returns>
    public bool TryShip(JobResultReport report, string masterAddress)
    {
        if (!_enabled || report == null) return false;
        return _channel.Writer.TryWrite(new ShipItem(report, masterAddress));
    }

    /// <summary>
    ///     启动后台消费者
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_enabled)
        {
            _logger.LogInformation("结果批量上报已关闭（ResultShipBatchSize=1），回调走逐条上报");
            return Task.CompletedTask;
        }

        _consumer = Task.Run(() => ConsumeAsync(_cts.Token), CancellationToken.None);
        _logger.LogInformation("结果上报队列已启动，容量={Capacity}，批量={BatchSize}，窗口={IntervalMs}ms，并发={Concurrency}",
            _options.Value.EffectiveResultShipQueueCapacity, _options.Value.ResultShipBatchSize,
            _options.Value.ResultShipIntervalMs, _concurrency);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     停止：把队列里剩余的结果尽量上报完（停机丢结果只能等调度端超时兜底，代价高）
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _channel.Writer.TryComplete();
        try
        {
            if (_consumer != null)
                await Task.WhenAny(_consumer, Task.Delay(TimeSpan.FromSeconds(5), cancellationToken));

            //  等仍在飞的批次发完：上限同样是 5s，超时交由账本兜底
            var waitDeadline = DateTime.UtcNow.AddSeconds(5);
            while (_shipGate.CurrentCount < _concurrency && DateTime.UtcNow < waitDeadline)
                await Task.Delay(50, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "结果上报队列停止时异常");
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
        _shipGate?.Dispose();
        _shipGate = null;
    }

    /// <summary>
    ///     消费循环：攒批 -> 按调度端地址分组上报
    /// </summary>
    private async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        var reader = _channel.Reader;
        var batchSize = Math.Max(1, _options.Value.ResultShipBatchSize);
        var intervalMs = Math.Max(200, _options.Value.ResultShipIntervalMs);

        try
        {
            while (await reader.WaitToReadAsync(cancellationToken))
            {
                var batch = new List<ShipItem>(batchSize);
                while (batch.Count < batchSize && reader.TryRead(out var item)) batch.Add(item);
                if (batch.Count == 0) continue;

                //  再等一个窗口，把同一波执行的结果合并成尽量少的 HTTP 请求
                var deadline = DateTime.UtcNow.AddMilliseconds(intervalMs);
                while (batch.Count < batchSize && DateTime.UtcNow < deadline && !cancellationToken.IsCancellationRequested)
                {
                    if (reader.TryRead(out var item))
                    {
                        batch.Add(item);
                        continue;
                    }

                    await Task.Delay(30, cancellationToken);
                }

                await ShipAsync(batch, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            //  正常停止
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "结果上报消费循环异常退出");
        }
    }

    /// <summary>
    ///     发送一批：通过闸门限制在飞批次数，闸门满时等待（对上游形成背压，不丢结果也不无限堆积）
    /// </summary>
    private async Task ShipAsync(List<ShipItem> batch, CancellationToken cancellationToken)
    {
        await _shipGate.WaitAsync(cancellationToken);
        _ = Task.Run(async () =>
        {
            try
            {
                await PostBatchAsync(batch, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                //  正常停止
            }
            catch (Exception ex)
            {
                Interlocked.Add(ref _failedBatches, 1);
                _logger.LogError(ex, "结果批量上报异常 {Count} 条，交由执行账本超时兜底", batch.Count);
            }
            finally
            {
                _shipGate.Release();
            }
        }, CancellationToken.None);
    }

    /// <summary>
    ///     批量上报：按调度端地址分组（跨调度端的批不能混发）
    /// </summary>
    private async Task PostBatchAsync(List<ShipItem> batch, CancellationToken cancellationToken)
    {
        foreach (var group in batch.GroupBy(x => x.MasterAddress ?? string.Empty))
        {
            var reports = group.Select(x => x.Report).ToList();
            if (await _masterChannel.ReportResultsAsync(group.Key, reports, cancellationToken))
            {
                Interlocked.Add(ref _postedResults, reports.Count);
                continue;
            }

            Interlocked.Add(ref _failedBatches, 1);
            _logger.LogWarning("结果批量上报失败 {Count} 条（协议 {Protocol}，累计失败批次 {Failed}），交由执行账本超时兜底",
                reports.Count, _masterChannel.Protocol, Interlocked.Read(ref _failedBatches));
        }
    }

    /// <summary>
    ///     队列元素：结果体 + 目标调度端地址
    /// </summary>
    private sealed class ShipItem
    {
        public ShipItem(JobResultReport report, string masterAddress)
        {
            Report = report;
            MasterAddress = masterAddress;
        }

        public JobResultReport Report { get; }

        public string MasterAddress { get; }
    }
}
