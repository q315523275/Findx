using System.Threading.Tasks;

namespace Findx.Notifications;

/// <summary>
///     用户通知发送器
///     <para>"给某个用户发一条通知"的最小契约，不绑定任何厂商通道：核心包只定义它，
///     具体走企业微信、钉钉、邮件、短信还是站内信，由扩展包实现并在容器里替换默认空实现</para>
///     <para>组件之间通过它通信，就不必互相引用：审批组件发待办不需要知道通知组件的存在</para>
/// </summary>
public interface IUserNotifier
{
    /// <summary>
    ///     发送通知
    /// </summary>
    /// <param name="notification">通知内容</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    Task SendAsync(UserNotification notification, CancellationToken cancellationToken = default);
}
