namespace Findx.Extensions.Scheduler.Models;

/// <summary>
///     执行状态
///     <para>Claimed=已在该节点领取但要等 Worker 确认受理；Dispatched=Worker 已受理等待回调；
///     Succeeded/Failed/Cancelled 为终态，不允许再迁移。</para>
/// </summary>
public enum ExecutionState
{
    /// <summary>
    ///     已领取，尚未确认受理（正常应在秒级迁移到 Dispatched）
    /// </summary>
    Claimed = 1,

    /// <summary>
    ///     执行节点已受理，等待结果回调
    /// </summary>
    Dispatched = 2,

    /// <summary>
    ///     执行成功
    /// </summary>
    Succeeded = 3,

    /// <summary>
    ///     执行失败（含执行超时兜底）
    /// </summary>
    Failed = 4,

    /// <summary>
    ///     被人工终止
    /// </summary>
    Cancelled = 5
}

/// <summary>
///     执行状态迁移规则（纯函数，便于单测）
/// </summary>
public static class ExecutionStateHelper
{
    /// <summary>
    ///     是否终态
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public static bool IsTerminal(ExecutionState state)
    {
        return state is ExecutionState.Succeeded or ExecutionState.Failed or ExecutionState.Cancelled;
    }

    /// <summary>
    ///     是否允许迁移
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public static bool CanTransit(ExecutionState from, ExecutionState to)
    {
        if (IsTerminal(from)) return false;

        return to switch
        {
            ExecutionState.Dispatched => from == ExecutionState.Claimed,
            ExecutionState.Succeeded => from is ExecutionState.Claimed or ExecutionState.Dispatched,
            ExecutionState.Failed => from is ExecutionState.Claimed or ExecutionState.Dispatched,
            ExecutionState.Cancelled => from is ExecutionState.Claimed or ExecutionState.Dispatched,
            _ => false
        };
    }
}
