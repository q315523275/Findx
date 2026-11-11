namespace Findx.Extensions.WorkflowCore.Enums;

/// <summary>
///     流程实例状态
/// </summary>
public enum FlwInstanceState
{
    /// <summary>
    ///     审批中
    /// </summary>
    Running = 0,

    /// <summary>
    ///     审批通过
    /// </summary>
    Approved = 1,

    /// <summary>
    ///     审批拒绝
    /// </summary>
    Rejected = 2,

    /// <summary>
    ///     撤销审批
    /// </summary>
    Revoked = 3,

    /// <summary>
    ///     超时结束
    /// </summary>
    Timeout = 4,

    /// <summary>
    ///     强制终止
    /// </summary>
    Terminated = 5
}
