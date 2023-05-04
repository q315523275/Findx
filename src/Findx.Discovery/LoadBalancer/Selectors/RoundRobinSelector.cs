using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Discovery.LoadBalancer.Selectors
{
    public class RoundRobinSelector : ILoadBalancer
    {
        private readonly string _serviceName;
        private readonly Func<Task<IList<IServiceInstance>>> _services;
        private int _last;

        public RoundRobinSelector(Func<Task<IList<IServiceInstance>>> services, string serviceName)
        {
            _services = services;
            _serviceName = serviceName;
        }

        public LoadBalancerType Name => LoadBalancerType.RoundRobin;

        public async Task<IServiceInstance> ResolveServiceInstanceAsync()
        {
            var services = await _services.Invoke();

            if (services == null)
                throw new ArgumentNullException($"{_serviceName}");

            if (!services.Any())
                throw new ArgumentNullException($"{_serviceName}");

            Interlocked.Increment(ref _last);

            if (_last >= services.Count) Interlocked.Exchange(ref _last, 0);

            return services[_last];
        }

        public Task UpdateStatsAsync(IServiceInstance serviceInstance, TimeSpan responseTime)
        {
            return Task.CompletedTask;
        }
    }
}