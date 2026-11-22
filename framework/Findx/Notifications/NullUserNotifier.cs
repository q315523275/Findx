using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Findx.Notifications;

/// <summary>
///     用户通知空实现
///     <para>Null 对象模式：宿主没接入任何通知通道时不实际发送，只记调试日志，
///     避免调用方为了"可能没人接"而到处判空</para>
/// </summary>
public class NullUserNotifier : IUserNotifier
{
    private readonly ILogger<NullUserNotifier> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="logger"></param>
    public NullUserNotifier(ILogger<NullUserNotifier> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     发送通知
    /// </summary>
    /// <param name="notification">通知内容</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public Task SendAsync(UserNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NullUserNotifier 不实际发送通知，接收人：{Receivers}，标题：{Title}",
            notification?.Receivers == null ? null : string.Join(",", notification.Receivers), notification?.Title);
        return Task.CompletedTask;
    }
}
