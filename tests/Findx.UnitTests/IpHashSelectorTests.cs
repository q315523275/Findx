using System.Collections.Concurrent;
using System.Reflection;
using Findx.Common.HashEx;
using Findx.DependencyInjection;
using Findx.Discovery.Abstractions;
using Findx.Discovery.LoadBalancer;
using Findx.Discovery.LoadBalancer.Selectors;
using Findx.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.UnitTests;

/// <summary>
///     IpHashSelector 回归测试（修复：哈希环全局 lock 串行化 → volatile 快照 + 变化才重建，热路径无锁）
/// </summary>
public class IpHashSelectorTests : IDisposable
{
    private sealed class FakeEndPoint : IServiceEndPoint
    {
        public FakeEndPoint(string host, int port = 8080)
        {
            Host = host;
            Port = port;
        }

        public string ServiceName => "test-service";

        public string Host { get; }

        public int Port { get; }

        public IDictionary<string, string> Metadata => new Dictionary<string, string>();

        public override bool Equals(object? obj)
        {
            return obj is FakeEndPoint other && Host == other.Host && Port == other.Port;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Host, Port);
        }
    }

    private sealed class FakeIpAccessor : IThreadCurrentClientIpAccessor
    {
        public string Ip { get; set; } = "10.1.2.3";

        public string GetClientIp()
        {
            return Ip;
        }
    }

    private static readonly FieldInfo ProviderField =
        typeof(ServiceLocator).GetField("_provider", BindingFlags.NonPublic | BindingFlags.Static)!;

    private readonly FakeIpAccessor _ipAccessor = new();

    private readonly ServiceProvider _provider;

    public IpHashSelectorTests()
    {
        //  选择器通过 ServiceLocator 获取客户端Ip，测试中注入固定Ip保证落点确定性
        _provider = new ServiceCollection()
            .AddSingleton<IThreadCurrentClientIpAccessor>(_ipAccessor)
            .BuildServiceProvider();
        ProviderField.SetValue(null, _provider);
    }

    public void Dispose()
    {
        ProviderField.SetValue(null, null);
        _provider.Dispose();
    }

    private static List<IServiceEndPoint> BuildEndPoints(params string[] hosts)
    {
        return hosts.Select(x => (IServiceEndPoint)new FakeEndPoint(x)).ToList();
    }

    private static ILoadBalancer CreateSelector(Func<IReadOnlyList<IServiceEndPoint>> factory)
    {
        return new IpHashSelector(() => Task.FromResult(factory()), "test-service");
    }

    /// <summary>
    ///     独立构建相同节点集合的一致性哈希环，用于校验选择结果与算法语义一致
    /// </summary>
    private static IServiceEndPoint ExpectedNode(IReadOnlyList<IServiceEndPoint> nodes, string ip)
    {
        var hash = new ConsistentHash<IServiceEndPoint>();
        hash.Initialize(nodes);
        return hash.GetItemNode(ip);
    }

    [Fact]
    public async Task Resolve_SingleInstance_AlwaysReturnsIt()
    {
        var endpoints = BuildEndPoints("10.0.0.1");
        var selector = CreateSelector(() => endpoints);

        for (var i = 0; i < 100; i++)
        {
            var endpoint = await selector.ResolveServiceEndPointAsync(default);
            Assert.Equal("10.0.0.1", endpoint.Host);
        }
    }

    [Fact]
    public async Task Resolve_SameIpAndSet_IsDeterministic()
    {
        var endpoints = BuildEndPoints("10.0.0.1", "10.0.0.2", "10.0.0.3");
        var selector = CreateSelector(() => endpoints);

        var expected = ExpectedNode(endpoints, _ipAccessor.Ip);
        for (var i = 0; i < 100; i++)
        {
            var endpoint = await selector.ResolveServiceEndPointAsync(default);
            Assert.Equal(expected.Host, endpoint.Host);
        }
    }

    [Fact]
    public async Task Resolve_AfterInstanceAdded_ReturnsNodeInCurrentSet()
    {
        var current = BuildEndPoints("10.0.0.1", "10.0.0.2");
        var selector = CreateSelector(() => current);

        //  先以 2 实例稳定运行
        for (var i = 0; i < 20; i++) await selector.ResolveServiceEndPointAsync(default);

        //  扩容到 3 实例
        current = BuildEndPoints("10.0.0.1", "10.0.0.2", "10.0.0.3");

        for (var i = 0; i < 200; i++)
        {
            _ipAccessor.Ip = $"10.1.0.{i}";
            var endpoint = await selector.ResolveServiceEndPointAsync(default);
            Assert.True(endpoint.Host is "10.0.0.1" or "10.0.0.2" or "10.0.0.3");
        }
    }

    [Fact]
    public async Task Resolve_AfterInstanceRemoved_NeverReturnsRemovedInstance()
    {
        var current = BuildEndPoints("10.0.0.1", "10.0.0.2", "10.0.0.3");
        var selector = CreateSelector(() => current);

        //  以 3 实例预热
        for (var i = 0; i < 200; i++)
        {
            _ipAccessor.Ip = $"10.1.0.{i}";
            await selector.ResolveServiceEndPointAsync(default);
        }

        //  下线 10.0.0.2（缩容为 2 实例）
        current = BuildEndPoints("10.0.0.1", "10.0.0.3");

        for (var i = 0; i < 200; i++)
        {
            _ipAccessor.Ip = $"10.1.0.{i}";
            var endpoint = await selector.ResolveServiceEndPointAsync(default);
            //  环必须已重建：不允许再命中已下线节点（环形残留节点）
            Assert.NotEqual("10.0.0.2", endpoint.Host);
            Assert.True(endpoint.Host is "10.0.0.1" or "10.0.0.3");
        }
    }

    [Fact]
    public async Task Resolve_RemovedInstance_MatchesRebuiltHashRing()
    {
        var current = BuildEndPoints("10.0.0.1", "10.0.0.2", "10.0.0.3");
        var selector = CreateSelector(() => current);
        await selector.ResolveServiceEndPointAsync(default);

        var remaining = BuildEndPoints("10.0.0.1", "10.0.0.3");
        current = remaining;

        for (var i = 0; i < 50; i++)
        {
            _ipAccessor.Ip = $"10.2.0.{i}";
            var endpoint = await selector.ResolveServiceEndPointAsync(default);
            var expected = ExpectedNode(remaining, _ipAccessor.Ip);
            Assert.Equal(expected.Host, endpoint.Host);
        }
    }

    [Fact]
    public async Task Resolve_EquivalentNewObjects_KeepsSameResult()
    {
        var selector = CreateSelector(() => BuildEndPoints("10.0.0.1", "10.0.0.2", "10.0.0.3"));
        _ipAccessor.Ip = "10.3.0.1";

        //  每次返回新对象但 Equals 相同：不应因“看似变化”导致结果漂移
        var first = (await selector.ResolveServiceEndPointAsync(default)).Host;
        for (var i = 0; i < 100; i++)
        {
            var endpoint = await selector.ResolveServiceEndPointAsync(default);
            Assert.Equal(first, endpoint.Host);
        }
    }

    [Fact]
    public async Task Resolve_Concurrent_StableSet_NoExceptionAndAlwaysInSet()
    {
        var endpoints = BuildEndPoints("10.0.0.1", "10.0.0.2", "10.0.0.3");
        var selector = CreateSelector(() => endpoints);
        var hosts = new ConcurrentBag<string>();

        var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(async () =>
        {
            for (var i = 0; i < 200; i++)
            {
                var endpoint = await selector.ResolveServiceEndPointAsync(default);
                hosts.Add(endpoint.Host);
            }
        }));

        await Task.WhenAll(tasks);

        Assert.Equal(4000, hosts.Count);
        Assert.All(hosts, host => Assert.True(host is "10.0.0.1" or "10.0.0.2" or "10.0.0.3"));
    }

    [Fact]
    public async Task Resolve_Concurrent_WhileInstanceSetChanges_AlwaysInUnion()
    {
        var two = BuildEndPoints("10.0.0.1", "10.0.0.2");
        var three = BuildEndPoints("10.0.0.1", "10.0.0.2", "10.0.0.3");
        var current = two;
        var selector = CreateSelector(() => current);
        var hosts = new ConcurrentBag<string>();
        var running = true;

        //  后台线程持续切换实例集合（模拟注册表变化）
        var switcher = Task.Run(async () =>
        {
            while (Volatile.Read(ref running))
            {
                current = three;
                await Task.Delay(1);
                current = two;
                await Task.Delay(1);
            }
        });

        var readers = Enumerable.Range(0, 10).Select(_ => Task.Run(async () =>
        {
            for (var i = 0; i < 200; i++)
            {
                var endpoint = await selector.ResolveServiceEndPointAsync(default);
                hosts.Add(endpoint.Host);
            }
        }));

        await Task.WhenAll(readers);
        Volatile.Write(ref running, false);
        await switcher;

        //  结果必须始终落在“切换前集合 ∪ 切换后集合”内，且并发重建不抛异常
        Assert.All(hosts, host => Assert.True(host is "10.0.0.1" or "10.0.0.2" or "10.0.0.3"));
    }

    [Fact]
    public async Task Resolve_EmptyOrNullInstances_Throws()
    {
        var selector = CreateSelector(() => new List<IServiceEndPoint>());
        await Assert.ThrowsAsync<ArgumentNullException>(() => selector.ResolveServiceEndPointAsync(default));

        var selectorForNull = CreateSelector(() => null!);
        await Assert.ThrowsAsync<ArgumentNullException>(() => selectorForNull.ResolveServiceEndPointAsync(default));
    }

    [Fact]
    public async Task UpdateStatsAsync_CompletesWithoutError()
    {
        var selector = CreateSelector(() => BuildEndPoints("10.0.0.1"));
        var endpoint = await selector.ResolveServiceEndPointAsync(default);
        await selector.UpdateStatsAsync(endpoint, TimeSpan.FromMilliseconds(10), default);
    }
}
