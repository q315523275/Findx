using System.Threading.Tasks;

namespace Findx.Discovery.LoadBalancer
{
    public interface ILoadBalancerProvider
    {
        Task<ILoadBalancer> GetAsync(string serviceName, LoadBalancerType loadBalancer = LoadBalancerType.Random);
    }
}
