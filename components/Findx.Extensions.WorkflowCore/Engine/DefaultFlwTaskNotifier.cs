using Findx.Notifications;
using Microsoft.Extensions.Logging;

namespace Findx.Extensions.WorkflowCore.Engine;

/// <summary>
///     默认任务通知实现：写运行时日志，并转成框架统一通知契约下发
///     <para>下发走 <see cref="IUserNotifier"/>：接入 Findx.Extensions.Notify 后自动落到企微/钉钉/邮件，
///     没接入时容器里是 <see cref="NullUserNotifier"/>，只记调试日志，不会因为"没配通道"中断流程</para>
///     <para>宿主需要自定义渠道或文案时，注册自己的 IFlwTaskNotifier 覆盖本实现</para>
/// </summary>
public class DefaultFlwTaskNotifier : IFlwTaskNotifier
{
    private readonly IUserNotifier _userNotifier;
    private readonly ILogger<DefaultFlwTaskNotifier> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="userNotifier">框架统一通知契约</param>
    /// <param name="logger"></param>
    public DefaultFlwTaskNotifier(IUserNotifier userNotifier, ILogger<DefaultFlwTaskNotifier> logger)
    {
        _userNotifier = userNotifier;
        _logger = logger;
    }

    /// <summary>
    ///     通知
    /// </summary>
    /// <param name="context">通知上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task NotifyAsync(FlwTaskNotifyContext context, CancellationToken cancellationToken = default)
    {
        var instance = context.Instance;
        var task = context.Task;
        var actors = context.Actors ?? [];

        _logger.LogInformation(
            "工作流任务{Kind}：实例[{InstanceNo}] 节点[{TaskName}] 办理人[{Actors}] 到期时间[{ExpireTime}]",
            context.Expired ? "已超时" : "即将到期",
            instance?.InstanceNo,
            task?.TaskName,
            string.Join(",", actors.Select(x => x.ActorName)),
            task?.ExpireTime);

        //  没有办理人就没有接收方，不必惊动通知组件
        var receivers = actors.Select(x => x.ActorId)
                              .Where(x => !string.IsNullOrWhiteSpace(x))
                              .Distinct()
                              .ToArray();
        if (receivers.Length == 0) return;

        var expireText = task?.ExpireTime?.ToString("yyyy-MM-dd HH:mm");
        await _userNotifier.SendAsync(new UserNotification
        {
            Receivers = receivers,
            Title = context.Expired ? "审批任务已超时" : "审批任务即将超时",
            Content = $"【{instance?.ProcessName}】{task?.TaskName} 待办理，单号 {instance?.BusinessCode ?? instance?.InstanceNo}，到期时间 {expireText}",
            Business = new BusinessAnchor
            {
                BusinessType = instance?.BusinessType,
                BusinessId = instance?.BusinessId.ToString(),
                BusinessCode = instance?.BusinessCode ?? instance?.InstanceNo,
                Title = instance?.ProcessName
            }
        }, cancellationToken);
    }
}
