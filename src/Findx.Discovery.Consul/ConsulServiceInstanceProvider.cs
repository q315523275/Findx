using Consul;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Discovery.Consul
{
    public class ConsulServiceInstanceProvider : IServiceInstanceProvider
    {
        public string ProviderName => "Consul";

        private readonly IConsulClient _client;

        public ConsulServiceInstanceProvider(IConsulClient client)
        {
            _client = client;
        }

        public async Task<IList<IServiceInstance>> GetInstancesAsync(string serviceName, string group = null, bool passingOnly = true, string tag = null, CancellationToken cancellationToken = default)
        {
            var instances = new List<IServiceInstance>();

            await AddInstancesToListAsync(instances, serviceName, tag, passingOnly, QueryOptions.Default, cancellationToken).ConfigureAwait(false);

            return instances;
        }

        internal async Task AddInstancesToListAsync(ICollection<IServiceInstance> instances, string serviceName, string tag = null, bool passingOnly = true, QueryOptions queryOptions = null, CancellationToken cancellationToken = default)
        {
            var result = await _client.Health.Service(serviceName, tag, passingOnly, queryOptions, cancellationToken).ConfigureAwait(false);
            var response = result.Response;

            foreach (var instance in response.Select(s => new ConsulServiceInstance(s)))
            {
                instances.Add(instance);
            }
        }

        public async Task<IList<string>> GetServicesAsync(string group = null, CancellationToken cancellationToken = default)
        {
            var result = await _client.Catalog.Services(QueryOptions.Default, cancellationToken).ConfigureAwait(false);
            var response = result.Response;
            return response.Keys.ToList();
        }
    }
}
