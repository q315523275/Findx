namespace Findx.Extensions.WorkflowCore.Enums;

/// <summary>
///     审批时间轴节点状态（审批组件展示用，与任务状态解耦）
/// </summary>
public enum FlwTimelineNodeState
{
    /// <summary>
    ///     未开始
    /// </summary>
    Pending = 0,

    /// <summary>
    ///     处理中（有待办任务）
    /// </summary>
    Running = 1,

    /// <summary>
    ///     已通过
    /// </summary>
    Approved = 2,

    /// <summary>
    ///     已驳回
    /// </summary>
    Rejected = 3,

    /// <summary>
    ///     已跳过（分支未命中 / 任务被撤销归档）
    /// </summary>
    Skipped = 4,

    /// <summary>
    ///     抄送（不阻塞流转）
    /// </summary>
    CarbonCopy = 5
}
