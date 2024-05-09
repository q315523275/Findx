using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Findx.Discovery.Abstractions;

namespace Findx.Discovery.LoadBalancer;

/// <summary>
/// 服务负载算法提供者
/// </summary>
public class LoadBalancerProvider : ILoadBalancerProvider
{
    private readonly IDiscoveryClient _discoveryClient;
    private readonly ILoadBalancerFactory _factory;
    private readonly ConcurrentDictionary<string, ILoadBalancer> _loadBalancers;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="discoveryClient"></param>
    /// <param name="factory"></param>
    public LoadBalancerProvider(IDiscoveryClient discoveryClient, ILoadBalancerFactory factory)
    {
        _discoveryClient = discoveryClient;
        _factory = factory;
        _loadBalancers = new ConcurrentDictionary<string, ILoadBalancer>();
    }

    /// <summary>
    /// 获取服务算法计算者
    /// </summary>
    /// <param name="serviceName"></param>
    /// <param name="loadBalancer"></param>
    /// <returns></returns>
    public async Task<ILoadBalancer> GetAsync(string serviceName, LoadBalancerType loadBalancer = LoadBalancerType.Random)
    {
        try
        {
            if (_loadBalancers.TryGetValue(serviceName, out var currentLoadBalancer))
            {
                if (loadBalancer != currentLoadBalancer.Name)
                {
                    currentLoadBalancer = await _factory.CreateAsync(serviceName, _discoveryClient, loadBalancer);
                    AddLoadBalancer(serviceName, currentLoadBalancer);
                }
                return currentLoadBalancer;
            }

            {
                currentLoadBalancer = await _factory.CreateAsync(serviceName, _discoveryClient, loadBalancer);
                AddLoadBalancer(serviceName, currentLoadBalancer);
                return currentLoadBalancer;
            }
        }
        catch (Exception ex)
        {
            throw new KeyNotFoundException($"unable to find load balancer for {serviceName} exception is {ex}");
        }
    }

    private void AddLoadBalancer(string key, ILoadBalancer loadBalancer)
    {
        _loadBalancers.AddOrUpdate(key, loadBalancer, (_, _) => loadBalancer);
    }
}