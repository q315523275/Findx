using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Findx.Discovery.LoadBalancer.Selectors
{
    public class NoLoadBalancerSelector : ILoadBalancer
    {
        private readonly IList<IServiceInstance> _services;

        public NoLoadBalancerSelector(IList<IServiceInstance> services)
        {
            _services = services;
        }

        public LoadBalancerType Name => LoadBalancerType.NoLoadBalancer;

        public Task<IServiceInstance> ResolveServiceInstanceAsync()
        {
            return Task.FromResult(_services.FirstOrDefault());
        }

        public Task UpdateStatsAsync(IServiceInstance serviceInstance, TimeSpan responseTime)
        {
            return Task.CompletedTask;
        }
    }
}
