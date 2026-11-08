using System.Collections.Concurrent;
using Findx.Discovery.Abstractions;
using Findx.Discovery.LoadBalancer;
using Findx.Discovery.LoadBalancer.Selectors;

namespace Findx.UnitTests;

/// <summary>
///     RoundRobinSelector 并发回归测试（修复：Interlocked.Increment 返回值丢弃导致的竞态与分布坍缩）
/// </summary>
public class RoundRobinSelectorTests
{
    private sealed class FakeEndPoint : IServiceEndPoint
    {
        public FakeEndPoint(string host)
        {
            Host = host;
        }

        public string ServiceName => "test-service";
        public string Host { get; }
        public int Port => 8080;
        public IDictionary<string, string> Metadata => new Dictionary<string, string>();
    }

    private static ILoadBalancer CreateSelector(int count)
    {
        var endpoints = Enumerable.Range(1, count).Select(i => (IServiceEndPoint)new FakeEndPoint($"10.0.0.{i}")).ToList();
        return new RoundRobinSelector(() => Task.FromResult<IReadOnlyList<IServiceEndPoint>>(endpoints), "test-service");
    }

    [Fact]
    public async Task Resolve_Concurrent_DistributesEvenly()
    {
        var selector = CreateSelector(5);
        var counter = new ConcurrentDictionary<string, int>();
        const int threads = 8;
        const int perThread = 5000;

        var tasks = Enumerable.Range(0, threads).Select(_ => Task.Run(async () =>
        {
            for (var i = 0; i < perThread; i++)
            {
                var endpoint = await selector.ResolveServiceEndPointAsync(default);
                counter.AddOrUpdate(endpoint.Host, 1, (_, v) => v + 1);
            }
        }));

        await Task.WhenAll(tasks);

        // 40000 次选择严格均分：每实例 8000（修复前会坍缩到第 0 实例或抛 IndexOutOfRange）
        Assert.Equal(5, counter.Count);
        foreach (var (_, count) in counter) Assert.Equal(perThread * threads / 5, count);
    }

    [Fact]
    public async Task Resolve_SingleInstance_NoCrash()
    {
        var selector = CreateSelector(1);

        for (var i = 0; i < 100; i++)
        {
            var endpoint = await selector.ResolveServiceEndPointAsync(default);
            Assert.Equal("10.0.0.1", endpoint.Host);
        }
    }
}
