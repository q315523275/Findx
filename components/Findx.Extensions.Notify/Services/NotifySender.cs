using Findx.Extensions.Notify.Entity;
using Findx.Extensions.Notify.Scheduling;
using Findx.Extensions.Notify.Storage;

namespace Findx.Extensions.Notify.Services;

/// <summary>
///     通知发送器实现：
///     <para>SendAsync：当前线程同步发送并返回结果（会占用调用方一次网络往返）</para>
///     <para>QueueAsync：只构造对象并入内存队列，零 IO 立即返回，发送与落库在后台线程完成</para>
/// </summary>
public class NotifySender : INotifySender
{
    private readonly NotifyOptions _options;
    private readonly NotifyChannelDispatcher _dispatcher;
    private readonly NotifyDispatchQueue _dispatchQueue;
    private readonly NotifyLogStore _logStore;
    private readonly ILogger<NotifySender> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public NotifySender(IOptions<NotifyOptions> options, NotifyChannelDispatcher dispatcher,
        NotifyDispatchQueue dispatchQueue, NotifyLogStore logStore, ILogger<NotifySender> logger)
    {
        _options = options.Value;
        _dispatcher = dispatcher;
        _dispatchQueue = dispatchQueue;
        _logStore = logStore;
        _logger = logger;
    }

    /// <summary>
    ///     同步发送
    /// </summary>
    public async Task<NotifyResult> SendAsync(NotifyMessage message, CancellationToken cancellationToken = default)
    {
        if (message == null) return NotifyResult.Fail(NotifyChannel.WeCom, "消息不能为空");

        var started = DateTime.Now;
        var result = await _dispatcher.SendAsync(message, cancellationToken);
        await PersistAsync(message, result, started, cancellationToken);
        return result;
    }

    /// <summary>
    ///     异步投递（业务线程首选）：对象构造 + 入队，无网络与数据库 IO
    /// </summary>
    public async Task<long> QueueAsync(NotifyMessage message, CancellationToken cancellationToken = default)
    {
        if (message == null) return 0;

        var log = NotifyLog.From(message, _options.NodeName);
        if (_dispatchQueue.TryEnqueue(log)) return log.Id;

        //  队列饱和：本次降级为同步发送，保证消息不丢（此时才在调用线程产生网络耗时）
        _logger.LogWarning("通知队列已满（容量 {Capacity}），本次降级为同步发送", _options.QueueCapacity);

        var started = DateTime.Now;
        var result = await _dispatcher.SendAsync(message, cancellationToken);
        var sendTime = DateTime.Now;
        log.Apply(result, sendTime, (long)(sendTime - started).TotalMilliseconds);
        if (_options.LogEnabled) await _logStore.PersistAsync(log, cancellationToken);
        return log.Id;
    }

    /// <summary>
    ///     同内容广播到多个通道（逐通道并行，各自独立落日志）
    /// </summary>
    public async Task<IReadOnlyList<NotifyResult>> SendAsync(IEnumerable<NotifyChannel> channels, NotifyMessage message,
        CancellationToken cancellationToken = default)
    {
        if (channels == null || message == null) return Array.Empty<NotifyResult>();

        var tasks = channels.Distinct().Select(channel =>
        {
            //  复制后改通道，避免并发写同一个消息对象
            var copy = message.Clone();
            copy.Channel = channel;
            return SendAsync(copy, cancellationToken);
        });

        return await Task.WhenAll(tasks);
    }

    /// <summary>
    ///     同步路径落日志
    /// </summary>
    private async Task PersistAsync(NotifyMessage message, NotifyResult result, DateTime started, CancellationToken cancellationToken)
    {
        if (!_options.LogEnabled) return;

        var log = NotifyLog.From(message, _options.NodeName);
        log.CreatedTime = started;
        var sendTime = DateTime.Now;
        log.Apply(result, sendTime, (long)(sendTime - started).TotalMilliseconds);
        await _logStore.PersistAsync(log, cancellationToken);
    }
}
