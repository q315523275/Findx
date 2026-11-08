using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Findx.Common;
using Microsoft.Extensions.Logging;

namespace Findx.Discovery.Consul;

public class ConsulServiceRegistry : IConsulServiceRegistry
{
    private const string Unknown = "UNKNOWN";
    private const string Up = "UP";
    private const string Down = "DOWN";
    private const string OutOfService = "OUT_OF_SERVICE";
    private readonly IConsulClient _client;
    private readonly ILogger<ConsulServiceRegistry> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="client"></param>
    /// <param name="logger"></param>
    public ConsulServiceRegistry(IConsulClient client, ILogger<ConsulServiceRegistry> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<bool> RegisterAsync(IConsulRegistration registration, CancellationToken cancellationToken = default)
    {
        Check.NotNull(registration, nameof(registration));

        var response = await _client.Agent.ServiceRegister(registration.Service, cancellationToken).ConfigureAwait(false);

        return response.StatusCode == HttpStatusCode.OK;
    }

    public async Task<bool> DeregisterAsync(IConsulRegistration registration, CancellationToken cancellationToken = default)
    {
        Check.NotNull(registration, nameof(registration));

        var response = await _client.Agent.ServiceDeregister(registration.InstanceId, cancellationToken).ConfigureAwait(false);

        return response.StatusCode == HttpStatusCode.OK;
    }

    public async Task<string> GetStatusAsync(IConsulRegistration registration, CancellationToken cancellationToken = default)
    {
        Check.NotNull(registration, nameof(registration));

        //  Health.Checks 按服务名查询（/v1/health/checks/:service），而非实例 ID
        var response = await _client.Health.Checks(registration.ServiceName, QueryOptions.Default, cancellationToken).ConfigureAwait(false);
        var checks = response.Response;

        foreach (var check in checks)
        {
            if (registration.InstanceId.Equals(check.ServiceID, StringComparison.OrdinalIgnoreCase) &&
                check.Name.Equals("Service Maintenance Mode", StringComparison.OrdinalIgnoreCase))
            {
                return OutOfService;
            }
        }
        
        return Up;
    }

    public async Task SetStatusAsync(IConsulRegistration registration, string status, CancellationToken cancellationToken = default)
    {
        Check.NotNull(registration, nameof(registration));

        if (OutOfService.Equals(status, StringComparison.OrdinalIgnoreCase))
        {
            await _client.Agent.EnableServiceMaintenance(registration.InstanceId, OutOfService, cancellationToken).ConfigureAwait(false);
        }
        else if (Up.Equals(status, StringComparison.OrdinalIgnoreCase))
        {
            await _client.Agent.DisableServiceMaintenance(registration.InstanceId, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            throw new ArgumentException($"Unknown status: {status}");
        }
    }
}