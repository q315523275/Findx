namespace Findx.Extensions.WorkflowCore.Enums;

/// <summary>
///     任务超时处理动作
/// </summary>
public enum FlwTimeoutAction
{
    /// <summary>
    ///     不处理（仅记录，缺省值，避免宿主未配置时静默改变数据）
    /// </summary>
    None = 0,

    /// <summary>
    ///     仅通知提醒
    /// </summary>
    Remind = 1,

    /// <summary>
    ///     以当前办理人名义自动通过
    /// </summary>
    AutoApprove = 2,

    /// <summary>
    ///     以当前办理人名义自动拒绝（结束流程）
    /// </summary>
    AutoReject = 3
}
