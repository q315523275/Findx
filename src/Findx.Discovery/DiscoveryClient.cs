using Findx.Caching;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Discovery.Consul
{
    public class DiscoveryClient : IDiscoveryClient
    {
        public string ProviderName { get; }

        private readonly IServiceInstanceProvider _serviceInstanceProvider;

        private readonly IOptionsMonitor<DiscoveryOptions> _options;

        private readonly ICacheProvider _cacheProvider;

        private readonly string ServiceInstancesKeyPrefix = "ServiceInstances:";

        public DiscoveryClient(IServiceInstanceProvider serviceInstanceProvider, IOptionsMonitor<DiscoveryOptions> options, ICacheProvider cacheProvider)
        {
            _serviceInstanceProvider = serviceInstanceProvider;
            _options = options;
            _cacheProvider = cacheProvider;
            ProviderName = _serviceInstanceProvider.ProviderName;
        }

        private DiscoveryOptions Options
        {
            get
            {
                if (_options != null)
                {
                    return _options.CurrentValue;
                }
                return default;
            }
        }

        public async Task<IList<IServiceInstance>> GetAllInstancesAsync(string group = null, CancellationToken cancellationToken = default)
        {
            List<IServiceInstance> allInstances = new List<IServiceInstance>();
            var serviceNames = await GetServicesAsync(group, cancellationToken);

            foreach (var serviceName in serviceNames)
            {
                var instances = await GetInstancesAsync(serviceName, group: group, cancellationToken: cancellationToken);

                allInstances.AddRange(instances);
            }

            return allInstances;
        }

        public async Task<IList<IServiceInstance>> GetInstancesAsync(string serviceName, string group = null, bool passingOnly = true, string tag = null, CancellationToken cancellationToken = default)
        {
            if (Options.Cache)
            {
                var _cache = _cacheProvider.Get(Options.CacheStrategy);

                var instanceData = await _cache.GetAsync<IList<IServiceInstance>>($"{ServiceInstancesKeyPrefix}{serviceName}").ConfigureAwait(false);
                if (instanceData != null && instanceData.Count > 0)
                {
                    return instanceData;
                }
            }

            var instances = await _serviceInstanceProvider.GetInstancesAsync(serviceName, group, passingOnly, tag, cancellationToken);

            if (Options.Cache)
            {
                var _cache = _cacheProvider.Get(Options.CacheStrategy);

                await _cache.AddAsync($"{ServiceInstancesKeyPrefix}{serviceName}", instances, TimeSpan.FromSeconds(Options.CacheTTL), cancellationToken);
            }

            return instances;
        }

        public Task<IEnumerable<string>> GetServicesAsync(string group = null, CancellationToken cancellationToken = default)
        {
            return _serviceInstanceProvider.GetServicesAsync(group, cancellationToken);
        }
    }
}
