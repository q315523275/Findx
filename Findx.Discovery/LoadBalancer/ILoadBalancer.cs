using Findx.Discovery.Abstractions;
using System;
using System.Threading.Tasks;

namespace Findx.Discovery.LoadBalancer
{
    public interface ILoadBalancer
    {
        LoadBalancerType Name { get; }
        Task<IServiceInstance> ResolveServiceInstanceAsync();
        Task UpdateStatsAsync(IServiceInstance serviceInstance, TimeSpan responseTime);
    }
}
