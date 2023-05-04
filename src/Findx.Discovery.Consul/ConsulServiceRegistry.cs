﻿using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Logging;

namespace Findx.Discovery.Consul
{
    public class ConsulServiceRegistry : IConsulServiceRegistry
    {
        private const string UNKNOWN = "UNKNOWN";
        private const string UP = "UP";
        private const string DOWN = "DOWN";
        private const string OUT_OF_SERVICE = "OUT_OF_SERVICE";
        private readonly IConsulClient _client;
        private readonly ILogger<ConsulServiceRegistry> _logger;

        public ConsulServiceRegistry(IConsulClient client, ILogger<ConsulServiceRegistry> logger)
        {
            _client = client;
            _logger = logger;
        }

        public void Dispose()
        {
        }

        public async Task<bool> RegisterAsync(IConsulRegistration registration,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(registration, nameof(registration));

            var response = await _client.Agent.ServiceRegister(registration.Service).ConfigureAwait(false);

            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> DeregisterAsync(IConsulRegistration registration,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(registration, nameof(registration));

            var response = await _client.Agent.ServiceDeregister(registration.InstanceId).ConfigureAwait(false);

            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<string> GetStatusAsync(IConsulRegistration registration,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(registration, nameof(registration));

            var response = await _client.Health.Checks(registration.InstanceId, QueryOptions.Default)
                .ConfigureAwait(false);
            var checks = response.Response;

            foreach (var check in checks)
                if (check.ServiceID.Equals(registration.InstanceId) &&
                    check.Name.Equals("Service Maintenance Mode", StringComparison.OrdinalIgnoreCase))
                    return OUT_OF_SERVICE;

            return UP;
        }

        public async Task SetStatusAsync(IConsulRegistration registration, string status,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(registration, nameof(registration));

            if (OUT_OF_SERVICE.Equals(status, StringComparison.OrdinalIgnoreCase))
                await _client.Agent.EnableServiceMaintenance(registration.InstanceId, OUT_OF_SERVICE)
                    .ConfigureAwait(false);
            else if (UP.Equals(status, StringComparison.OrdinalIgnoreCase))
                await _client.Agent.DisableServiceMaintenance(registration.InstanceId).ConfigureAwait(false);
            else
                throw new ArgumentException($"Unknown status: {status}");
        }
    }
}