using Findx.Discovery.Abstractions;
using Findx.Discovery.LoadBalancer.Selectors;
using System.Threading.Tasks;

namespace Findx.Discovery.LoadBalancer
{
    public class LoadBalancerFactory : ILoadBalancerFactory
    {
        public async Task<ILoadBalancer> GetAsync(string serviceName, IDiscoveryClient discoveryClient, LoadBalancerType loadBalancer = LoadBalancerType.Random)
        {
            switch (loadBalancer)
            {
                case LoadBalancerType.Random:
                    return new RandomSelector(async () => await discoveryClient.GetInstancesAsync(serviceName), serviceName);
                case LoadBalancerType.RoundRobin:
                    return new RoundRobinSelector(async () => await discoveryClient.GetInstancesAsync(serviceName), serviceName);
                case LoadBalancerType.LeastConnection:
                    return new LeastConnectionSelector(async () => await discoveryClient.GetInstancesAsync(serviceName), serviceName);
                default:
                    return new NoLoadBalancerSelector(await discoveryClient.GetInstancesAsync(serviceName));
            }
        }
    }
}
