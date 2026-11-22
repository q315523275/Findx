namespace Findx.Extensions.Notify.ServiceDefaults;

/// <summary>
///     通知发送器：同步发送（返回结果）与异步投递（入队即返回）两种模式
/// </summary>
public interface INotifySender
{
    /// <summary>
    ///     同步发送：在当前线程完成投递并返回结果，通道/网络耗时（最长 TimeoutSeconds）会算在调用方身上。
    ///     仅用于"必须立刻知道成败"的场景，常规业务请用 QueueAsync
    /// </summary>
    Task<NotifyResult> SendAsync(NotifyMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    ///     异步投递：只做对象构造 + 入内存队列，零 IO、毫秒级返回；实际发送与日志落库在后台消费线程完成。
    ///     返回日志编号（0 表示消息为空未投递）；队列满时自动降级为同步发送，不丢消息
    /// </summary>
    Task<long> QueueAsync(NotifyMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    ///     同一条内容广播到多个通道（逐通道并行，单通道失败不影响其它）
    /// </summary>
    Task<IReadOnlyList<NotifyResult>> SendAsync(IEnumerable<NotifyChannel> channels, NotifyMessage message, CancellationToken cancellationToken = default);
}
