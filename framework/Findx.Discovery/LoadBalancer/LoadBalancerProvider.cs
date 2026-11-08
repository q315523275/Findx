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
            // 无负载均衡策略每次按需解析最新端点，不做进程级缓存，避免进程级快照不刷新
            if (loadBalancer == LoadBalancerType.NoLoadBalancer)
                return await _factory.CreateAsync(serviceName, _discoveryClient, loadBalancer);

            if (_loadBalancers.TryGetValue(serviceName, out var currentLoadBalancer) && currentLoadBalancer.Name == loadBalancer)
                return currentLoadBalancer;

            currentLoadBalancer = await _factory.CreateAsync(serviceName, _discoveryClient, loadBalancer);

            //  AddOrUpdate 原子写入，并发首调最坏多创建一个选择器实例
            return _loadBalancers.AddOrUpdate(serviceName, currentLoadBalancer, (_, _) => currentLoadBalancer);
        }
        catch (Exception ex)
        {
            throw new KeyNotFoundException($"unable to find load balancer for {serviceName} exception is {ex}", ex);
        }
    }

}