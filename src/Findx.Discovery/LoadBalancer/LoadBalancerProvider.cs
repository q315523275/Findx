using Findx.Discovery.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Findx.Discovery.LoadBalancer
{
    public class LoadBalancerProvider : ILoadBalancerProvider
    {
        private readonly IDiscoveryClient _discoveryClient;
        private readonly ILoadBalancerFactory _factory;
        private readonly ConcurrentDictionary<string, ILoadBalancer> _loadBalancers;

        public LoadBalancerProvider(IDiscoveryClient discoveryClient, ILoadBalancerFactory factory)
        {
            _discoveryClient = discoveryClient;
            _factory = factory;
            _loadBalancers = new ConcurrentDictionary<string, ILoadBalancer>();
        }

        public async Task<ILoadBalancer> GetAsync(string serviceName, LoadBalancerType _loadBalancer = LoadBalancerType.Random)
        {
            try
            {
                if (_loadBalancers.TryGetValue(serviceName, out var loadBalancer))
                {
                    loadBalancer = _loadBalancers[serviceName];
                    if (_loadBalancer != loadBalancer.Name)
                    {
                        loadBalancer = await _factory.GetAsync(serviceName, _discoveryClient, _loadBalancer);
                        AddLoadBalancer(serviceName, loadBalancer);
                    }
                    return loadBalancer;
                }
                {
                    loadBalancer = await _factory.GetAsync(serviceName, _discoveryClient, _loadBalancer);
                    AddLoadBalancer(serviceName, loadBalancer);
                    return loadBalancer;
                }
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException($"unabe to find load balancer for {serviceName} exception is {ex}");
            }
        }

        private void AddLoadBalancer(string key, ILoadBalancer loadBalancer)
        {
            _loadBalancers.AddOrUpdate(key, loadBalancer, (x, y) => loadBalancer);
        }
    }
}
