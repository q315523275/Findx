using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Findx.Common.HashEx;
using Findx.DependencyInjection;
using Findx.Discovery.Abstractions;
using Findx.Threading;
using Findx.Utilities;

namespace Findx.Discovery.LoadBalancer.Selectors;

/// <summary>
///     客户端Ip Hash选择器
/// </summary>
public class IpHashSelector : ILoadBalancer
{
    /// <summary>
    ///     一致性哈希环快照：环与对应的实例集合整体发布，读取侧单引用读取即保证二者一致
    /// </summary>
    private sealed class RingSnapshot
    {
        public RingSnapshot(ConsistentHash<IServiceEndPoint> ring, IReadOnlyList<IServiceEndPoint> instances)
        {
            Ring = ring;
            Instances = instances;
            InstanceSet = new HashSet<IServiceEndPoint>(instances);
        }

        public ConsistentHash<IServiceEndPoint> Ring { get; }

        public IReadOnlyList<IServiceEndPoint> Instances { get; }

        /// <summary>
        ///     实例集合索引：稳态比较 O(m) 且零分配
        /// </summary>
        public HashSet<IServiceEndPoint> InstanceSet { get; }
    }

    private static readonly RingSnapshot EmptySnapshot = new(new ConsistentHash<IServiceEndPoint>(), []);
    private readonly Func<Task<IReadOnlyList<IServiceEndPoint>>> _services;
    private readonly string _serviceName;
    private readonly object _rebuildLock = new();

    /// <summary>
    ///     当前环快照。volatile：实例增减时重建并整体替换引用，保证其他线程及时看到新快照
    /// </summary>
    private volatile RingSnapshot _snapshot = EmptySnapshot;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceName"></param>
    public IpHashSelector(Func<Task<IReadOnlyList<IServiceEndPoint>>> services, string serviceName)
    {
        _services = services;
        _serviceName = serviceName;
    }

    /// <summary>
    ///     选择器名
    /// </summary>
    public LoadBalancerType Name => LoadBalancerType.IpHash;

    /// <summary>
    ///     获取服务
    /// </summary>
    /// <returns></returns>
    public async Task<IServiceEndPoint> ResolveServiceEndPointAsync(CancellationToken cancellationToken = default)
    {
        var services = await _services.Invoke();

        if (services == null || services.Count == 0)
            throw new ArgumentNullException($"{_serviceName}");

        //  客户端Ip,不存在客户端Ip时使用本地Ip
        var clientIp = ServiceLocator.IsProviderEnabled ? ServiceLocator.GetService<IThreadCurrentClientIpAccessor>()?.GetClientIp() : null;
        if (string.IsNullOrWhiteSpace(clientIp))
            clientIp = HostUtility.ResolveHostAddress(HostUtility.ResolveHostName());

        //  稳态热路径：实例集合与环快照一致时直接读快照，无需加锁（快照发布后不再修改，并发只读安全）
        var snapshot = _snapshot;
        if (IsSameInstances(snapshot, services))
            return snapshot.Ring.GetItemNode(clientIp);

        //  实例集合发生变化：按当前实例一次性重建哈希环（重建后环内不存在已下线节点），双检避免并发重复重建
        lock (_rebuildLock)
        {
            if (!IsSameInstances(_snapshot, services))
            {
                var newRing = new ConsistentHash<IServiceEndPoint>();
                newRing.Initialize(services);
                _snapshot = new RingSnapshot(newRing, services);
            }
        }

        return _snapshot.Ring.GetItemNode(clientIp);
    }

    /// <summary>
    ///     比较实例集合是否一致（IServiceEndPoint 实现方均为值语义 Equals）
    /// </summary>
    private static bool IsSameInstances(RingSnapshot snapshot, IReadOnlyList<IServiceEndPoint> services)
    {
        if (ReferenceEquals(snapshot.Instances, services)) return true;
        if (snapshot.InstanceSet.Count != services.Count) return false;

        for (var i = 0; i < services.Count; i++)
        {
            var item = services[i];
            if (item == null || !snapshot.InstanceSet.Contains(item)) return false;
        }

        return true;
    }

    /// <summary>
    ///     更新服务统计
    /// </summary>
    /// <param name="serviceEndPoint"></param>
    /// <param name="responseTime"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task UpdateStatsAsync(IServiceEndPoint serviceEndPoint, TimeSpan responseTime, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
