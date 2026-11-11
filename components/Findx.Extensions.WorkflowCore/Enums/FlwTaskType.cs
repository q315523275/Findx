namespace Findx.Extensions.WorkflowCore.Enums;

/// <summary>
///     任务类型
/// </summary>
public enum FlwTaskType
{
    /// <summary>
    ///     审批任务
    /// </summary>
    Approval = 0,

    /// <summary>
    ///     抄送任务（只读，不阻塞流程）
    /// </summary>
    CarbonCopy = 1,

    /// <summary>
    ///     触发器任务（等待外部完成后继续流转）
    /// </summary>
    Trigger = 2
}
