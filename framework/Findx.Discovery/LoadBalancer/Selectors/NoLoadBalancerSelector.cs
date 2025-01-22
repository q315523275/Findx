using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Findx.Discovery.Abstractions;

namespace Findx.Discovery.LoadBalancer.Selectors;

/// <summary>
/// 无负载均衡选择器
/// </summary>
public class NoLoadBalancerSelector : ILoadBalancer
{
    private readonly IReadOnlyList<IServiceEndPoint> _services;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="services"></param>
    public NoLoadBalancerSelector(IReadOnlyList<IServiceEndPoint> services)
    {
        _services = services;
    }

    /// <summary>
    /// 选择器名
    /// </summary>
    public LoadBalancerType Name => LoadBalancerType.NoLoadBalancer;

    /// <summary>
    /// 获取服务
    /// </summary>
    /// <returns></returns>
    public Task<IServiceEndPoint> ResolveServiceEndPointAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_services.FirstOrDefault());
    }

    /// <summary>
    /// 更新服务统计
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