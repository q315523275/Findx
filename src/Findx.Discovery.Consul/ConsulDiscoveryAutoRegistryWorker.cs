using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Findx.Discovery.Consul
{
    public class ConsulDiscoveryAutoRegistryWorker : BackgroundService
    {
        private readonly IOptionsMonitor<DiscoveryOptions> _options;
        private readonly IConsulRegistration _registration;
        private readonly IConsulServiceRegistry _registry;

        public ConsulDiscoveryAutoRegistryWorker(IOptionsMonitor<DiscoveryOptions> options,
            IConsulServiceRegistry registry, IConsulRegistration registration)
        {
            _options = options;
            _registry = registry;
            _registration = registration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.CurrentValue.Enabled || !_options.CurrentValue.Register)
                return;

            await _registry.RegisterAsync(_registration, stoppingToken).ConfigureAwait(false);
        }
    }
}