namespace Findx.Extensions.WorkflowCore.Enums;

/// <summary>
///     流程节点类型（与 FlowLong 节点模型 type 取值保持一致）
/// </summary>
public enum FlwNodeType
{
    /// <summary>
    ///     发起节点（流程根节点）
    /// </summary>
    Root = 0,

    /// <summary>
    ///     审批节点
    /// </summary>
    Approval = 1,

    /// <summary>
    ///     抄送节点（不阻塞流转）
    /// </summary>
    CarbonCopy = 2,

    /// <summary>
    ///     条件分支（排它，只走一个满足条件的分支）
    /// </summary>
    Condition = 3,

    /// <summary>
    ///     并行分支（不解析条件，所有分支并行执行，汇聚等待全部完成）
    /// </summary>
    Parallel = 4,

    /// <summary>
    ///     包容分支（解析条件，执行所有满足条件的分支，汇聚只等待被选中的分支）
    /// </summary>
    Inclusive = 5,

    /// <summary>
    ///     路由分支（按条件重定向到指定节点，用于驳回/复审回路）
    /// </summary>
    Route = 6,

    /// <summary>
    ///     触发器节点（执行自定义逻辑或延时任务）
    /// </summary>
    Trigger = 7,

    /// <summary>
    ///     子流程节点（调用另一个流程定义）
    /// </summary>
    CallProcess = 8
}
