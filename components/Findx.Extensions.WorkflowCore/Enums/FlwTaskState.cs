namespace Findx.Extensions.WorkflowCore.Enums;

/// <summary>
///     任务状态
/// </summary>
public enum FlwTaskState
{
    /// <summary>
    ///     活动（待处理）
    /// </summary>
    Active = 0,

    /// <summary>
    ///     跳转
    /// </summary>
    Jump = 1,

    /// <summary>
    ///     完成
    /// </summary>
    Completed = 2,

    /// <summary>
    ///     拒绝
    /// </summary>
    Rejected = 3,

    /// <summary>
    ///     撤销
    /// </summary>
    Revoked = 4,

    /// <summary>
    ///     超时
    /// </summary>
    Timeout = 5,

    /// <summary>
    ///     终止
    /// </summary>
    Terminated = 6,

    /// <summary>
    ///     驳回终止
    /// </summary>
    RejectTerminated = 7
}
