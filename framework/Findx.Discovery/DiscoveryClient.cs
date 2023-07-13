using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Findx.Caching;
using Microsoft.Extensions.Options;

namespace Findx.Discovery;

/// <summary>
/// 服务发现客户端
/// </summary>
public class DiscoveryClient : IDiscoveryClient
{
    private readonly ICacheFactory _cacheFactory;

    private readonly IOptionsMonitor<DiscoveryOptions> _options;

    private readonly IServiceInstanceProvider _serviceInstanceProvider;

    private const string ServiceInstancesKeyPrefix = "ServiceInstances:";

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="serviceInstanceProvider"></param>
    /// <param name="options"></param>
    /// <param name="cacheFactory"></param>
    public DiscoveryClient(IServiceInstanceProvider serviceInstanceProvider, IOptionsMonitor<DiscoveryOptions> options, ICacheFactory cacheFactory)
    {
        _serviceInstanceProvider = serviceInstanceProvider;
        _options = options;
        _cacheFactory = cacheFactory;
        ProviderName = _serviceInstanceProvider.ProviderName;
    }

    /// <summary>
    /// 配置
    /// </summary>
    private DiscoveryOptions Options => _options?.CurrentValue;

    /// <summary>
    /// 服务名称
    /// </summary>
    public string ProviderName { get; }

    /// <summary>
    /// 获取所有实例
    /// </summary>
    /// <param name="group"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IList<IServiceInstance>> GetAllInstancesAsync(string group = null, CancellationToken cancellationToken = default)
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

    /// <summary>
    /// 获取服务实例
    /// </summary>
    /// <param name="serviceName"></param>
    /// <param name="group"></param>
    /// <param name="passingOnly"></param>
    /// <param name="tag"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IList<IServiceInstance>> GetInstancesAsync(string serviceName, string group = null, bool passingOnly = true, string tag = null, CancellationToken cancellationToken = default)
    {
        if (Options.Cache)
        {
            var cache = _cacheFactory.Create(Options.CacheStrategy);

            var instanceData = await cache
                .GetAsync<IList<IServiceInstance>>($"{ServiceInstancesKeyPrefix}{serviceName}", cancellationToken)
                .ConfigureAwait(false);
            if (instanceData is { Count: > 0 }) 
                return instanceData;
        }

        var instances =
            await _serviceInstanceProvider.GetInstancesAsync(serviceName, group, passingOnly, tag,
                cancellationToken);

        if (!Options.Cache) return instances;
        {
            var cache = _cacheFactory.Create(Options.CacheStrategy);

            await cache.AddAsync($"{ServiceInstancesKeyPrefix}{serviceName}", instances,
                TimeSpan.FromSeconds(Options.CacheTtl), cancellationToken);
        }

        return instances;
    }

    /// <summary>
    /// 获取服务实例
    /// </summary>
    /// <param name="group"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IEnumerable<string>> GetServicesAsync(string group = null,
        CancellationToken cancellationToken = default)
    {
        return _serviceInstanceProvider.GetServicesAsync(group, cancellationToken);
    }
}