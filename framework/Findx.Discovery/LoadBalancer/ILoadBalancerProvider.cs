using System.Threading.Tasks;
using Findx.Discovery.LoadBalancer;

namespace Findx.Discovery
{
    /// <summary>
    ///     负载均衡器提供者
    /// </summary>
    public interface ILoadBalancerProvider
    {
        /// <summary>
        ///     获取负载均衡器
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="loadBalancer"></param>
        /// <returns></returns>
        Task<ILoadBalancer> GetAsync(string serviceName, LoadBalancerType loadBalancer = LoadBalancerType.Random);
    }
}