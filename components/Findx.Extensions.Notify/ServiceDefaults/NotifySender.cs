using Findx.Extensions.Notify.Entity;
using Findx.Extensions.Notify.Scheduling;
using Findx.Extensions.Notify.Storage;
using Findx.NewId;
using Findx.Notifications;

namespace Findx.Extensions.Notify.ServiceDefaults;

/// <summary>
///     通知发送器实现：
///     <para>SendAsync：当前线程同步发送并返回结果（会占用调用方一次网络往返）</para>
///     <para>QueueAsync：只构造对象并入内存队列，零 IO 立即返回，发送与落库在后台线程完成</para>
///     <para>同时实现核心包的 IUserNotifier，供审批等组件按通用契约发通知，不必引用本组件</para>
/// </summary>
public class NotifySender : INotifySender, IUserNotifier
{
    private readonly NotifyOptions _options;
    private readonly NotifyChannelDispatcher _dispatcher;
    private readonly NotifyDispatchQueue _dispatchQueue;
    private readonly NotifyLogStore _logStore;
    private readonly IKeyGenerator<long> _keyGenerator;
    private readonly ILogger<NotifySender> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public NotifySender(IOptions<NotifyOptions> options, NotifyChannelDispatcher dispatcher, NotifyDispatchQueue dispatchQueue, NotifyLogStore logStore
        , IKeyGenerator<long> keyGenerator, ILogger<NotifySender> logger)
    {
        _options = options.Value;
        _dispatcher = dispatcher;
        _dispatchQueue = dispatchQueue;
        _logStore = logStore;
        _keyGenerator = keyGenerator;
        _logger = logger;
    }

    /// <summary>
    ///     同步发送
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<long> QueueAsync(NotifyMessage message, CancellationToken cancellationToken = default)
    {
        if (message == null) return 0;

        var log = NotifyLog.From(message, _options.NodeName, _keyGenerator.Create());
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
    /// <param name="channels"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IReadOnlyList<NotifyResult>> SendAsync(IEnumerable<NotifyChannel> channels, NotifyMessage message, CancellationToken cancellationToken = default)
    {
        if (channels == null || message == null) return [];

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
    ///     通用用户通知（框架 IUserNotifier 契约）：按配置的默认通道广播
    ///     <para>审批、调度这类组件只依赖核心包契约，由本方法决定落到哪些通道</para>
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    public async Task SendAsync(UserNotification notification, CancellationToken cancellationToken = default)
    {
        if (notification == null) return;

        //  契约字段映射到通道消息：BusinessCode 作业务关联键，Url 供通道跳转
        var message = new NotifyMessage
        {
            To = notification.Receivers,
            Title = notification.Title,
            Content = notification.Content,
            IsMarkdown = notification.IsMarkdown,
            Url = notification.Business?.Url,
            BusinessKey = notification.Business?.BusinessCode ?? notification.Business?.BusinessId,
            Data = notification.Data == null ? null : new Dictionary<string, object>(notification.Data)
        };

        var channels = _options.DefaultChannels is { Length: > 0 } ? _options.DefaultChannels : [NotifyChannel.WeCom];
        if (channels.Length == 1)
        {
            message.Channel = channels[0];
            await SendAsync(message, cancellationToken);
            return;
        }

        await SendAsync(channels, message, cancellationToken);
    }

    /// <summary>
    ///     同步路径落日志
    /// </summary>
    /// <param name="message"></param>
    /// <param name="result"></param>
    /// <param name="started"></param>
    /// <param name="cancellationToken"></param>
    private async Task PersistAsync(NotifyMessage message, NotifyResult result, DateTime started, CancellationToken cancellationToken)
    {
        if (!_options.LogEnabled) return;

        var log = NotifyLog.From(message, _options.NodeName, _keyGenerator.Create());
        log.CreatedTime = started;
        var sendTime = DateTime.Now;
        log.Apply(result, sendTime, (long)(sendTime - started).TotalMilliseconds);
        await _logStore.PersistAsync(log, cancellationToken);
    }
}
