using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Findx.DependencyInjection;
using Findx.Discovery.Abstractions;

namespace Findx.Discovery.Consul;

public class ConsulServiceEndPointProvider : IServiceEndPointProvider, IServiceNameAware
{
    private readonly IConsulClient _client;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="client"></param>
    public ConsulServiceEndPointProvider(IConsulClient client)
    {
        _client = client;
    }

    public string Name => "Consul";

    public async Task<IReadOnlyList<IServiceEndPoint>> GetServiceEndPointsAsync(string serviceName, string group = null, bool passingOnly = true, string tag = null, CancellationToken cancellationToken = default)
    {
        var instances = new List<IServiceEndPoint>();
        await AddInstancesToListAsync(instances, serviceName, tag, passingOnly, QueryOptions.Default, cancellationToken).ConfigureAwait(false);
        return instances;
    }

    public async Task<IEnumerable<string>> GetServicesAsync(string group = null, CancellationToken cancellationToken = default)
    {
        var result = await _client.Catalog.Services(QueryOptions.Default, cancellationToken).ConfigureAwait(false);
        var response = result.Response;
        return response.Keys.AsEnumerable();
    }

    private async Task AddInstancesToListAsync(ICollection<IServiceEndPoint> instances, string serviceName, string tag = null, bool passingOnly = true, QueryOptions queryOptions = null, CancellationToken cancellationToken = default)
    {
        var result = await _client.Health.Service(serviceName, tag, passingOnly, queryOptions, cancellationToken).ConfigureAwait(false);
        var response = result.Response;

        foreach (var instance in response.Select(s => new ConsulServiceEndPoint(s))) instances.Add(instance);
    }
}