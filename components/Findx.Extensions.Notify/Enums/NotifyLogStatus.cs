namespace Findx.Extensions.Notify.Enums;

/// <summary>
///     通知发送状态
/// </summary>
public enum NotifyLogStatus
{
    /// <summary>
    ///     已入队待发送
    /// </summary>
    Queued = 0,

    /// <summary>
    ///     发送成功
    /// </summary>
    Success = 1,

    /// <summary>
    ///     发送失败
    /// </summary>
    Failed = 2
}
