using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.Threading;
using Findx.Utilities;

namespace Findx.Discovery.LoadBalancer.Selectors;

/// <summary>
/// 客户端Ip Hash选择器
/// </summary>
public class IpHashSelector : ILoadBalancer
{
    private readonly ConsistentHash<IServiceInstance> _serviceInstanceNodes = new();
    private readonly string _serviceName;
    private readonly Func<Task<IList<IServiceInstance>>> _services;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceName"></param>
    public IpHashSelector(Func<Task<IList<IServiceInstance>>> services, string serviceName)
    {
        _services = services;
        _serviceName = serviceName;
    }

    /// <summary>
    /// 选择器名
    /// </summary>
    public LoadBalancerType Name => LoadBalancerType.IpHash;

    /// <summary>
    /// 获取服务
    /// </summary>
    /// <returns></returns>
    public async Task<IServiceInstance> ResolveServiceInstanceAsync()
    {
        var services = await _services.Invoke();

        if (services == null)
            throw new ArgumentNullException($"{_serviceName}");

        if (!services.Any())
            throw new ArgumentNullException($"{_serviceName}");

        // 添加不存在服务
        foreach (var service in services)
        {
            if (!_serviceInstanceNodes.ContainNode(service))
                _serviceInstanceNodes.Add(service);
        }
        
        // 客户端Ip,不存在客户端Ip时使用本地Ip
        var clientIp = ServiceLocator.GetService<IThreadCurrentClientIpAccessor>()?.GetClientIp()?? HostUtility.ResolveHostAddress(HostUtility.ResolveHostName());
        
        var currentService = (IServiceInstance) null;
        while (currentService == null || !services.Contains(currentService))
        {
            // 移除hash环节点
            if (currentService != null)
                _serviceInstanceNodes.Remove(currentService);
            
            currentService = _serviceInstanceNodes.GetItemNode(clientIp);
        }
        return currentService;
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