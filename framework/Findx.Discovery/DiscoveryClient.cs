using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Findx.Caching;
using Microsoft.Extensions.Options;

namespace Findx.Discovery.Consul
{
    public class DiscoveryClient : IDiscoveryClient
    {
        private readonly ICacheFactory _cacheFactory;

        private readonly IOptionsMonitor<DiscoveryOptions> _options;

        private readonly IServiceInstanceProvider _serviceInstanceProvider;

        private readonly string ServiceInstancesKeyPrefix = "ServiceInstances:";

        public DiscoveryClient(IServiceInstanceProvider serviceInstanceProvider, IOptionsMonitor<DiscoveryOptions> options, ICacheFactory cacheFactory)
        {
            _serviceInstanceProvider = serviceInstanceProvider;
            _options = options;
            _cacheFactory = cacheFactory;
            ProviderName = _serviceInstanceProvider.ProviderName;
        }

        private DiscoveryOptions Options
        {
            get
            {
                if (_options != null) return _options.CurrentValue;
                return default;
            }
        }

        public string ProviderName { get; }

        public async Task<IList<IServiceInstance>> GetAllInstancesAsync(string group = null,
            CancellationToken cancellationToken = default)
        {
            var allInstances = new List<IServiceInstance>();
            var serviceNames = await GetServicesAsync(group, cancellationToken);

            foreach (var serviceName in serviceNames)
            {
                var instances = await GetInstancesAsync(serviceName, group, cancellationToken: cancellationToken);

                allInstances.AddRange(instances);
            }

            return allInstances;
        }

        public async Task<IList<IServiceInstance>> GetInstancesAsync(string serviceName, string group = null,
            bool passingOnly = true, string tag = null, CancellationToken cancellationToken = default)
        {
            if (Options.Cache)
            {
                var cache = _cacheFactory.Create(Options.CacheStrategy);

                var instanceData = await cache
                    .GetAsync<IList<IServiceInstance>>($"{ServiceInstancesKeyPrefix}{serviceName}", cancellationToken)
                    .ConfigureAwait(false);
                if (instanceData != null && instanceData.Count > 0) return instanceData;
            }

            var instances =
                await _serviceInstanceProvider.GetInstancesAsync(serviceName, group, passingOnly, tag,
                    cancellationToken);

            if (Options.Cache)
            {
                var cache = _cacheFactory.Create(Options.CacheStrategy);

                await cache.AddAsync($"{ServiceInstancesKeyPrefix}{serviceName}", instances,
                    TimeSpan.FromSeconds(Options.CacheTtl), cancellationToken);
            }

            return instances;
        }

        public Task<IEnumerable<string>> GetServicesAsync(string group = null,
            CancellationToken cancellationToken = default)
        {
            return _serviceInstanceProvider.GetServicesAsync(group, cancellationToken);
        }
    }
}