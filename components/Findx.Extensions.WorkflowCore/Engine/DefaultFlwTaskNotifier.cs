using Microsoft.Extensions.Logging;

namespace Findx.Extensions.WorkflowCore.Engine;

/// <summary>
///     默认任务通知实现：只写日志，宿主可注册自己的 IFlwTaskNotifier 接管
/// </summary>
public class DefaultFlwTaskNotifier : IFlwTaskNotifier
{
    private readonly ILogger<DefaultFlwTaskNotifier> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public DefaultFlwTaskNotifier(ILogger<DefaultFlwTaskNotifier> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     通知
    /// </summary>
    public Task NotifyAsync(FlwTaskNotifyContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "工作流任务{Kind}：实例[{InstanceNo}] 节点[{TaskName}] 办理人[{Actors}] 到期时间[{ExpireTime}]",
            context.Expired ? "已超时" : "即将到期",
            context.Instance?.InstanceNo,
            context.Task?.TaskName,
            string.Join(",", context.Actors?.Select(x => x.ActorName) ?? []),
            context.Task?.ExpireTime);
        return Task.CompletedTask;
    }
}
