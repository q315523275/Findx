using Findx.Extensions.WorkflowCore.Entity;
using Findx.Extensions.WorkflowCore.Models;

namespace Findx.Extensions.WorkflowCore.Engine;

/// <summary>
///     节点办理人解析扩展点
///     <para>各平台的组织模型不同（用户/角色/部门/上级/发起人可能有不同的数据来源），宿主可实现该接口从自己的组织架构取人；未实现时使用内置默认实现。</para>
/// </summary>
public interface IFlwActorProvider
{
    /// <summary>
    ///     解析节点办理人
    /// </summary>
    /// <param name="context">解析上下文（实例、节点、发起人）</param>
    /// <param name="cancellationToken">取消标记</param>
    /// <returns>任务参与者（ActorType 决定匹配方式：用户直配 / 角色 / 部门）</returns>
    Task<List<FlwTaskActorInfo>> ResolveAsync(FlwActorResolveContext context, CancellationToken cancellationToken = default);
}

/// <summary>
///     办理人解析上下文
/// </summary>
public class FlwActorResolveContext
{
    /// <summary>
    ///     流程实例
    /// </summary>
    public FlwInstanceInfo Instance { get; set; }

    /// <summary>
    ///     当前节点（node.Props.Assignee 为模型上配置的办理人）
    /// </summary>
    public FlwNodeModel Node { get; set; }

    /// <summary>
    ///     发起人
    /// </summary>
    public FlwCreator Creator { get; set; }
}
