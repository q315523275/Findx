using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly ConsistentHash<IServiceEndPoint> _serviceInstanceNodes = new();
    private readonly Func<Task<IReadOnlyList<IServiceEndPoint>>> _services;
    private readonly string _serviceName;

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
        
        var currentService = (IServiceEndPoint) null;
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