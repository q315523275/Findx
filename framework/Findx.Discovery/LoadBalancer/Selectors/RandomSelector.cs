using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Findx.Discovery.Abstractions;
using Findx.Utilities;

namespace Findx.Discovery.LoadBalancer.Selectors;

/// <summary>
/// 随机选择器
/// </summary>
public class RandomSelector : ILoadBalancer
{
    private readonly Func<int, int, int> _generate;
    private readonly string _serviceName;
    private readonly Func<Task<IReadOnlyList<IServiceEndPoint>>> _services;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceName"></param>
    public RandomSelector(Func<Task<IReadOnlyList<IServiceEndPoint>>> services, string serviceName)
    {
        _services = services;
        _serviceName = serviceName;
        _generate = RandomUtility.RandomInt;
    }

    /// <summary>
    /// 选择器名
    /// </summary>
    public LoadBalancerType Name => LoadBalancerType.Random;

    /// <summary>
    /// 获取服务
    /// </summary>
    /// <returns></returns>
    public async Task<IServiceEndPoint> ResolveServiceEndPointAsync()
    {
        var services = await _services.Invoke();

        if (services == null)
            throw new ArgumentNullException($"{_serviceName}");

        if (!services.Any())
            throw new ArgumentNullException($"{_serviceName}");

        var index = _generate(0, services.Count);

        return services[index];
    }

    /// <summary>
    /// 更新服务统计
    /// </summary>
    /// <param name="serviceEndPoint"></param>
    /// <param name="responseTime"></param>
    /// <returns></returns>
    public Task UpdateStatsAsync(IServiceEndPoint serviceEndPoint, TimeSpan responseTime)
    {
        return Task.CompletedTask;
    }
}