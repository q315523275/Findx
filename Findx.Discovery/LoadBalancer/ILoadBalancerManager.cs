using System.Threading.Tasks;

namespace Findx.Discovery.LoadBalancer
{
    public interface ILoadBalancerManager
    {
        Task<ILoadBalancer> GetAsync(string serviceName, LoadBalancerType loadBalancer = LoadBalancerType.Random);
    }
}
