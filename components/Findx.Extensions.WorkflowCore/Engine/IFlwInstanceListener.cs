using Findx.Extensions.WorkflowCore.Entity;
using Findx.Extensions.WorkflowCore.Enums;

namespace Findx.Extensions.WorkflowCore.Engine;

/// <summary>
///     流程实例完结上下文
/// </summary>
public class FlwInstanceFinishedContext
{
    /// <summary>
    ///     流程实例
    /// </summary>
    public FlwInstanceInfo Instance { get; set; }

    /// <summary>
    ///     业务锚点（未注册适配器时为 null）
    /// </summary>
    public FlwBusinessInfo Business { get; set; }

    /// <summary>
    ///     结束状态：Approved / Rejected / Revoked / Timeout / Terminated
    /// </summary>
    public FlwInstanceState State { get; set; }

    /// <summary>
    ///     耗时（秒）
    /// </summary>
    public decimal Duration { get; set; }
}

/// <summary>
///     流程实例监听器：事务提交后触发，适合发消息、推送、刷新缓存等副作用。
///     <para>与 <see cref="IFlwBusinessAdapter" /> 的区别：这里在提交之后执行，失败不影响流程与业务数据</para>
/// </summary>
public interface IFlwInstanceListener
{
    /// <summary>
    ///     实例完结（仅在最终态触发一次）
    /// </summary>
    Task OnInstanceFinishedAsync(FlwInstanceFinishedContext context, CancellationToken cancellationToken = default);
}
