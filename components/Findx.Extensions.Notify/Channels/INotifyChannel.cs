namespace Findx.Extensions.Notify.Channels;

/// <summary>
///     通知通道抽象。新增通道只需实现本接口并在模块中按配置注册，通知发送器自动路由
/// </summary>
public interface INotifyChannel
{
    /// <summary>
    ///     通道标识
    /// </summary>
    NotifyChannel Channel { get; }

    /// <summary>
    ///     发送一条消息
    /// </summary>
    Task<NotifyResult> SendAsync(NotifyMessage message, CancellationToken cancellationToken = default);
}
