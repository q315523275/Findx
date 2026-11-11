using Findx.Extensions.WorkflowCore.Entity;

namespace Findx.Extensions.WorkflowCore.Engine;

/// <summary>
///     任务通知扩展点：提醒与超时的消息下发给宿主自己的渠道（站内信、企微、邮件等）
/// </summary>
public interface IFlwTaskNotifier
{
    /// <summary>
    ///     通知
    /// </summary>
    /// <param name="context">通知上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task NotifyAsync(FlwTaskNotifyContext context, CancellationToken cancellationToken = default);
}

/// <summary>
///     任务通知上下文
/// </summary>
public class FlwTaskNotifyContext
{
    /// <summary>
    ///     流程实例
    /// </summary>
    public FlwInstanceInfo Instance { get; set; }

    /// <summary>
    ///     任务
    /// </summary>
    public FlwTaskInfo Task { get; set; }

    /// <summary>
    ///     当前任务参与者
    /// </summary>
    public List<FlwTaskActorInfo> Actors { get; set; } = [];

    /// <summary>
    ///     true 表示已到期（超时），false 表示临近到期的提醒
    /// </summary>
    public bool Expired { get; set; }
}
