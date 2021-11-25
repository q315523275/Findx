using Findx.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Findx.Discovery.LoadBalancer.Selectors
{
    public class RandomSelector : ILoadBalancer
    {
        private readonly Func<Task<IList<IServiceInstance>>> _services;
        private readonly string _serviceName;
        private readonly Func<int, int, int> _generate;
        public RandomSelector(Func<Task<IList<IServiceInstance>>> services, string serviceName)
        {
            _services = services;
            _serviceName = serviceName;
            _generate = (min, max) => RandomUtil.RandomInt(min, max);
        }

        public LoadBalancerType Name => LoadBalancerType.Random;

        public async Task<IServiceInstance> ResolveServiceInstanceAsync()
        {
            var services = await _services.Invoke();

            if (services == null)
                throw new ArgumentNullException($"{_serviceName}");

            if (!services.Any())
                throw new ArgumentNullException($"{_serviceName}");

            var index = _generate(0, services.Count());

            return services[index];
        }

        public Task UpdateStatsAsync(IServiceInstance serviceInstance, TimeSpan responseTime)
        {
            return Task.CompletedTask;
        }
    }
}
