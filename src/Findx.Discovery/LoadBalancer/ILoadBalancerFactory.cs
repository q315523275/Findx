using Findx.Discovery.Abstractions;
using System.Threading.Tasks;

namespace Findx.Discovery.LoadBalancer
{
    public interface ILoadBalancerFactory
    {
        Task<ILoadBalancer> GetAsync(string serviceName, IDiscoveryClient discoveryClient, LoadBalancerType loadBalancerType = LoadBalancerType.Random);
    }
}
