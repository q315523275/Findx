using Findx.Extensions.Scheduler.Models;

namespace Findx.UnitTests;

/// <summary>
///     执行状态机纯函数测试：终态判定 + 允许迁移矩阵。
///     <para>这组规则是结果回调幂等与超时兜底互斥的唯一依据——回调与兜底都靠
///     "能否把行从 Claimed/Dispatched 迁到终态"来抢占终结权，规则错一条就会重复重试或丢结果。</para>
/// </summary>
public class SchedulerExecutionStateTests
{
    [Theory]
    [InlineData(ExecutionState.Succeeded, true)]
    [InlineData(ExecutionState.Failed, true)]
    [InlineData(ExecutionState.Cancelled, true)]
    [InlineData(ExecutionState.Claimed, false)]
    [InlineData(ExecutionState.Dispatched, false)]
    public void IsTerminal_MatchesDefinition(ExecutionState state, bool expected)
    {
        Assert.Equal(expected, ExecutionStateHelper.IsTerminal(state));
    }

    [Theory]
    [InlineData(ExecutionState.Claimed, ExecutionState.Dispatched, true)]
    [InlineData(ExecutionState.Claimed, ExecutionState.Failed, true)]
    [InlineData(ExecutionState.Claimed, ExecutionState.Succeeded, true)]
    [InlineData(ExecutionState.Claimed, ExecutionState.Cancelled, true)]
    [InlineData(ExecutionState.Dispatched, ExecutionState.Succeeded, true)]
    [InlineData(ExecutionState.Dispatched, ExecutionState.Failed, true)]
    [InlineData(ExecutionState.Dispatched, ExecutionState.Cancelled, true)]
    public void CanTransit_FromNonTerminal_Allowed(ExecutionState from, ExecutionState to, bool expected)
    {
        Assert.Equal(expected, ExecutionStateHelper.CanTransit(from, to));
    }

    [Fact]
    public void CanTransit_ToClaimed_AlwaysRejected()
    {
        // Claimed 是入场态，任何回退到 Claimed 都会让同一周期被再次分发
        foreach (var from in Enum.GetValues<ExecutionState>())
        {
            Assert.False(ExecutionStateHelper.CanTransit(from, ExecutionState.Claimed));
        }
    }

    [Theory]
    [InlineData(ExecutionState.Succeeded)]
    [InlineData(ExecutionState.Failed)]
    [InlineData(ExecutionState.Cancelled)]
    public void CanTransit_FromTerminal_AnyTargetRejected(ExecutionState from)
    {
        // 终态不可再迁移：迟到的回调、兜底或人工操作都不能改写已终结的结果
        foreach (var to in Enum.GetValues<ExecutionState>())
        {
            Assert.False(ExecutionStateHelper.CanTransit(from, to));
        }
    }

    [Theory]
    [InlineData(ExecutionState.Dispatched, ExecutionState.Dispatched)]
    [InlineData(ExecutionState.Claimed, ExecutionState.Claimed)]
    public void CanTransit_SameState_Rejected(ExecutionState from, ExecutionState to)
    {
        Assert.False(ExecutionStateHelper.CanTransit(from, to));
    }

    [Fact]
    public void CanTransit_FromFailedToSucceeded_Rejected()
    {
        // 失败已终结：迟到的成功回调不能再把状态翻回成功，否则 TryCount 归零会吞掉已记录的失败
        Assert.False(ExecutionStateHelper.CanTransit(ExecutionState.Failed, ExecutionState.Succeeded));
    }

    [Fact]
    public void CanTransit_FromCancelledToFailed_Rejected()
    {
        // 人工终止后执行节点仍可能回调失败，必须被拒，避免再次进入重试
        Assert.False(ExecutionStateHelper.CanTransit(ExecutionState.Cancelled, ExecutionState.Failed));
    }

    [Fact]
    public void CanTransit_FromSucceededToFailed_Rejected()
    {
        Assert.False(ExecutionStateHelper.CanTransit(ExecutionState.Succeeded, ExecutionState.Failed));
    }

    [Fact]
    public void CanTransit_FromCancelledToSucceeded_Rejected()
    {
        Assert.False(ExecutionStateHelper.CanTransit(ExecutionState.Cancelled, ExecutionState.Succeeded));
    }
}
