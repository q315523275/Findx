using Consul;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Discovery.Consul
{
    public class ConsulServiceRegistry : IConsulServiceRegistry
    {
        private readonly IConsulClient _client;
        private readonly ILogger<ConsulServiceRegistry> _logger;
        private const string UNKNOWN = "UNKNOWN";
        private const string UP = "UP";
        private const string DOWN = "DOWN";
        private const string OUT_OF_SERVICE = "OUT_OF_SERVICE";

        public ConsulServiceRegistry(IConsulClient client, ILogger<ConsulServiceRegistry> logger)
        {
            _client = client;
            _logger = logger;
        }

        public void Dispose()
        {

        }

        public async Task<bool> Register(IConsulRegistration registration, CancellationToken cancellationToken = default)
        {
            Check.NotNull(registration, nameof(registration));

            var response = await _client.Agent.ServiceRegister(registration.Service).ConfigureAwait(false);

            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> Deregister(IConsulRegistration registration, CancellationToken cancellationToken = default)
        {
            Check.NotNull(registration, nameof(registration));

            var response = await _client.Agent.ServiceDeregister(registration.InstanceId).ConfigureAwait(false);

            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<string> GetStatus(IConsulRegistration registration, CancellationToken cancellationToken = default)
        {
            Check.NotNull(registration, nameof(registration));

            var response = await _client.Health.Checks(registration.InstanceId, QueryOptions.Default).ConfigureAwait(false);
            var checks = response.Response;

            foreach (HealthCheck check in checks)
            {
                if (check.ServiceID.Equals(registration.InstanceId) && check.Name.Equals("Service Maintenance Mode", StringComparison.OrdinalIgnoreCase))
                {
                    return OUT_OF_SERVICE;
                }
            }

            return UP;
        }

        public async Task SetStatus(IConsulRegistration registration, string status, CancellationToken cancellationToken = default)
        {
            Check.NotNull(registration, nameof(registration));

            if (OUT_OF_SERVICE.Equals(status, StringComparison.OrdinalIgnoreCase))
            {
                await _client.Agent.EnableServiceMaintenance(registration.InstanceId, OUT_OF_SERVICE).ConfigureAwait(false);
            }
            else if (UP.Equals(status, StringComparison.OrdinalIgnoreCase))
            {
                await _client.Agent.DisableServiceMaintenance(registration.InstanceId).ConfigureAwait(false);
            }
            else
            {
                throw new ArgumentException($"Unknown status: {status}");
            }
        }
    }
}
