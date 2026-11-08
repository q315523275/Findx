using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Findx.Discovery.Abstractions;

namespace Findx.Discovery.LoadBalancer.Selectors;

/// <summary>
/// 轮询选择器
/// </summary>
public class RoundRobinSelector : ILoadBalancer
{
    private readonly string _serviceName;
    private readonly Func<Task<IReadOnlyList<IServiceEndPoint>>> _services;
    private int _last;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceName"></param>
    public RoundRobinSelector(Func<Task<IReadOnlyList<IServiceEndPoint>>> services, string serviceName)
    {
        _services = services;
        _serviceName = serviceName;
    }

    /// <summary>
    /// 选择器名称
    /// </summary>
    public LoadBalancerType Name => LoadBalancerType.RoundRobin;

    /// <summary>
    /// 获取实例
    /// </summary>
    /// <returns></returns>
    public async Task<IServiceEndPoint> ResolveServiceEndPointAsync(CancellationToken cancellationToken = default)
    {
        var services = await _services.Invoke();

        if (services == null)
            throw new ArgumentNullException($"{_serviceName}");

        if (!services.Any())
            throw new ArgumentNullException($"{_serviceName}");

        var n = Interlocked.Increment(ref _last);

        //  按自增返回值取模定位，避免“自增后再判断/重置”与“读取 _last 索引”之间的多线程竞态；
        //  兼容 int 长期自增溢出回绕后取模得到负数下标的情况
        var index = (n - 1) % services.Count;

        if (index < 0) index += services.Count;

        return services[index];
    }

    /// <summary>
    /// 更新统计
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