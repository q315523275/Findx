using System.Threading.Tasks;
using Findx.Discovery.Abstractions;
using Findx.Discovery.LoadBalancer.Selectors;

namespace Findx.Discovery.LoadBalancer;

/// <summary>
/// 服务负载算法工厂
/// </summary>
public class LoadBalancerFactory : ILoadBalancerFactory
{
    /// <summary>
    /// 创建服务负载算法
    /// </summary>
    /// <param name="serviceName"></param>
    /// <param name="discoveryClient"></param>
    /// <param name="loadBalancer"></param>
    /// <returns></returns>
    public async Task<ILoadBalancer> CreateAsync(string serviceName, IDiscoveryClient discoveryClient, LoadBalancerType loadBalancer = LoadBalancerType.Random)
    {
        switch (loadBalancer)
        {
            case LoadBalancerType.Random:
                return new RandomSelector(async () => await discoveryClient.GetServiceEndPointsAsync(serviceName), serviceName);
            case LoadBalancerType.RoundRobin:
                return new RoundRobinSelector(async () => await discoveryClient.GetServiceEndPointsAsync(serviceName), serviceName);
            case LoadBalancerType.LeastConnection:
                return new LeastConnectionSelector(async () => await discoveryClient.GetServiceEndPointsAsync(serviceName), serviceName);
            case LoadBalancerType.IpHash:
                return new IpHashSelector(async () => await discoveryClient.GetServiceEndPointsAsync(serviceName), serviceName);
            default:
                return new NoLoadBalancerSelector(await discoveryClient.GetServiceEndPointsAsync(serviceName));
        }
    }
}