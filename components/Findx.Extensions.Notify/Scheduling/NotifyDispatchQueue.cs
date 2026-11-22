using Findx.Extensions.Notify.Entity;
using Findx.Extensions.Notify.ServiceDefaults;
using Findx.Extensions.Notify.Storage;
using Microsoft.Extensions.Hosting;

namespace Findx.Extensions.Notify.Scheduling;

/// <summary>
///     异步发送队列：有界内存 Channel + 后台消费线程。
///     业务线程只做 TryWrite（无网络、无数据库往返），发送与日志落库都在这里完成
/// </summary>
public class NotifyDispatchQueue : BackgroundService
{
    private readonly Channel<NotifyLog> _channel;
    private readonly NotifyOptions _options;
    private readonly NotifyChannelDispatcher _dispatcher;
    private readonly NotifyLogStore _logStore;
    private readonly ILogger<NotifyDispatchQueue> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="dispatcher"></param>
    /// <param name="logStore"></param>
    /// <param name="logger"></param>
    public NotifyDispatchQueue(IOptions<NotifyOptions> options, NotifyChannelDispatcher dispatcher, NotifyLogStore logStore, ILogger<NotifyDispatchQueue> logger)
    {
        _options = options.Value;
        _dispatcher = dispatcher;
        _logStore = logStore;
        _logger = logger;

        _channel = Channel.CreateBounded<NotifyLog>(new BoundedChannelOptions(Math.Max(1, _options.QueueCapacity))
        {
            SingleReader = false,
            SingleWriter = false,
            //  满时 TryWrite 直接返回 false，由调用方决定降级策略，不做阻塞
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    /// <summary>
    ///     入队（非阻塞）。返回 false 表示队列已满
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    public bool TryEnqueue(NotifyLog log)
    {
        return log != null && _channel.Writer.TryWrite(log);
    }

    /// <summary>
    ///     启动消费线程
    /// </summary>
    /// <param name="stoppingToken"></param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var workerCount = Math.Max(1, _options.DispatchWorkerCount);
        var tasks = Enumerable.Range(0, workerCount).Select(_ => ConsumeAsync(stoppingToken)).ToArray();
        await Task.WhenAll(tasks);
    }

    /// <summary>
    ///     消费循环：发送 -> 回填结果 -> 落日志
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var log in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                var started = DateTime.Now;
                NotifyResult result;

                try
                {
                    result = await _dispatcher.SendAsync(log.ToMessage(), cancellationToken);
                }
                catch (Exception ex)
                {
                    result = NotifyResult.Fail(log.Channel, ex.Message);
                }

                var sendTime = DateTime.Now;
                log.Apply(result, sendTime, (long)(sendTime - started).TotalMilliseconds);

                if (_options.LogEnabled) await _logStore.PersistAsync(log, cancellationToken);

                if (!result.Success)
                    _logger.LogWarning("异步通知发送失败：Channel={Channel}，Reason={Reason}", log.Channel, result.Message);
            }
        }
        catch (OperationCanceledException)
        {
            //  停机时退出，队列内未发送消息随进程结束丢弃（属日志级数据，可接受）
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "通知异步发送消费循环异常退出");
        }
    }
}
