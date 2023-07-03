using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Findx.Discovery.LoadBalancer.Selectors;

/// <summary>
/// 无负载均衡选择器
/// </summary>
public class NoLoadBalancerSelector : ILoadBalancer
{
    private readonly IList<IServiceInstance> _services;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="services"></param>
    public NoLoadBalancerSelector(IList<IServiceInstance> services)
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
    public Task<IServiceInstance> ResolveServiceInstanceAsync()
    {
        return Task.FromResult(_services.FirstOrDefault());
    }

    /// <summary>
    /// 更新服务统计
    /// </summary>
    /// <param name="serviceInstance"></param>
    /// <param name="responseTime"></param>
    /// <returns></returns>
    public Task UpdateStatsAsync(IServiceInstance serviceInstance, TimeSpan responseTime)
    {
        return Task.CompletedTask;
    }
}