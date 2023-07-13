using System.Threading.Tasks;

namespace Findx.Discovery.LoadBalancer
{
    /// <summary>
    ///     负载均衡器创建工厂
    /// </summary>
    public interface ILoadBalancerFactory
    {
        /// <summary>
        ///     创建服务负载均衡器
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="discoveryClient"></param>
        /// <param name="loadBalancerType"></param>
        /// <returns></returns>
        Task<ILoadBalancer> CreateAsync(string serviceName, IDiscoveryClient discoveryClient,
            LoadBalancerType loadBalancerType = LoadBalancerType.Random);
    }
}