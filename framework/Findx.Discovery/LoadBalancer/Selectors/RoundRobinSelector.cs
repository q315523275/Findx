using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Discovery.LoadBalancer.Selectors;

/// <summary>
/// 轮询选择器
/// </summary>
public class RoundRobinSelector : ILoadBalancer
{
    private readonly string _serviceName;
    private readonly Func<Task<IList<IServiceInstance>>> _services;
    private int _last;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceName"></param>
    public RoundRobinSelector(Func<Task<IList<IServiceInstance>>> services, string serviceName)
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
    public async Task<IServiceInstance> ResolveServiceInstanceAsync()
    {
        var services = await _services.Invoke();

        if (services == null)
            throw new ArgumentNullException($"{_serviceName}");

        if (!services.Any())
            throw new ArgumentNullException($"{_serviceName}");

        Interlocked.Increment(ref _last);

        if (_last >= services.Count) Interlocked.Exchange(ref _last, 0);

        return services[_last];
    }

    /// <summary>
    /// 更新统计
    /// </summary>
    /// <param name="serviceInstance"></param>
    /// <param name="responseTime"></param>
    /// <returns></returns>
    public Task UpdateStatsAsync(IServiceInstance serviceInstance, TimeSpan responseTime)
    {
        return Task.CompletedTask;
    }
}