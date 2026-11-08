using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Scheduling;

namespace Findx.UnitTests;

/// <summary>
///     RoundRobin 分发策略纯逻辑测试：按 JobName 独立游标轮转
/// </summary>
public class RoundRobinDispatchStrategyTests
{
    private static WorkerNodeInfo NewNode(string id)
    {
        return new WorkerNodeInfo { WorkerId = id, Name = id, Address = $"http://{id}:5000", Capabilities = new List<string> { "job" } };
    }

    [Fact]
    public void Select_EmptyCandidates_ReturnsNull()
    {
        var strategy = new RoundRobinDispatchStrategy();
        var result = strategy.Select(new List<WorkerNodeInfo>(), "any-job");
        Assert.Null(result);
    }

    [Fact]
    public void Select_NullCandidates_ReturnsNull()
    {
        var strategy = new RoundRobinDispatchStrategy();
        Assert.Null(strategy.Select(null, "any-job"));
    }

    [Fact]
    public void Select_SingleCandidate_AlwaysReturnsIt()
    {
        var only = NewNode("worker-a");
        var strategy = new RoundRobinDispatchStrategy();
        var candidates = new List<WorkerNodeInfo> { only };

        for (var i = 0; i < 5; i++)
            Assert.Same(only, strategy.Select(candidates, "job-x"));
    }

    [Fact]
    public void Select_SameJobKey_RoundsRobinInOrder()
    {
        var a = NewNode("worker-a");
        var b = NewNode("worker-b");
        var candidates = new List<WorkerNodeInfo> { a, b };
        var strategy = new RoundRobinDispatchStrategy();

        Assert.Same(a, strategy.Select(candidates, "job-x"));
        Assert.Same(b, strategy.Select(candidates, "job-x"));
        Assert.Same(a, strategy.Select(candidates, "job-x"));
        Assert.Same(b, strategy.Select(candidates, "job-x"));
    }

    [Fact]
    public void Select_DifferentJobKeys_KeepIndependentCursors()
    {
        var a = NewNode("worker-a");
        var b = NewNode("worker-b");
        var candidates = new List<WorkerNodeInfo> { a, b };
        var strategy = new RoundRobinDispatchStrategy();

        // job-x 轮转两次：0,1
        Assert.Same(a, strategy.Select(candidates, "job-x"));
        Assert.Same(b, strategy.Select(candidates, "job-x"));

        // job-y 独立游标，从头开始
        Assert.Same(a, strategy.Select(candidates, "job-y"));
        Assert.Same(b, strategy.Select(candidates, "job-y"));

        // job-x 继续其游标
        Assert.Same(a, strategy.Select(candidates, "job-x"));
    }

    [Fact]
    public void Select_NullJobKey_DoesNotThrow()
    {
        var a = NewNode("worker-a");
        var b = NewNode("worker-b");
        var candidates = new List<WorkerNodeInfo> { a, b };
        var strategy = new RoundRobinDispatchStrategy();

        Assert.NotNull(strategy.Select(candidates, null));
        Assert.NotNull(strategy.Select(candidates, null));
    }
}
