namespace Findx.Extensions.WorkflowCore.Enums;

/// <summary>
///     任务处理动作（审计与监听使用）
/// </summary>
public enum FlwTaskAction
{
    /// <summary>
    ///     同意
    /// </summary>
    Agree = 0,

    /// <summary>
    ///     拒绝
    /// </summary>
    Reject = 1,

    /// <summary>
    ///     撤销（发起人撤回流程）
    /// </summary>
    Revoke = 2,

    /// <summary>
    ///     转办
    /// </summary>
    Transfer = 3,

    /// <summary>
    ///     加签
    /// </summary>
    AddSign = 4,

    /// <summary>
    ///     减签
    /// </summary>
    ReduceSign = 5,

    /// <summary>
    ///     跳转
    /// </summary>
    Jump = 6,

    /// <summary>
    ///     自动完成（无人办理或触发器回调）
    /// </summary>
    AutoComplete = 7
}
