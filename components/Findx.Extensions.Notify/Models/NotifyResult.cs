namespace Findx.Extensions.Notify.Models;

/// <summary>
///     通知发送结果
/// </summary>
public class NotifyResult
{
    /// <summary>
    ///     是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    ///     通道
    /// </summary>
    public NotifyChannel Channel { get; set; }

    /// <summary>
    ///     结果说明（成功为 ok，失败为可读原因）
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     构造成功结果
    /// </summary>
    public static NotifyResult Ok(NotifyChannel channel, string message = "ok")
    {
        return new NotifyResult { Success = true, Channel = channel, Message = message };
    }

    /// <summary>
    ///     构造失败结果
    /// </summary>
    public static NotifyResult Fail(NotifyChannel channel, string message)
    {
        return new NotifyResult { Success = false, Channel = channel, Message = message };
    }
}
