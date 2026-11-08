using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Findx.Caching;
using Findx.DependencyInjection;
using Findx.Discovery.Abstractions;
using Microsoft.Extensions.Options;

namespace Findx.Discovery;

/// <summary>
///     服务发现客户端
/// </summary>
public class DiscoveryClient : IDiscoveryClient
{
    private readonly ICacheFactory _cacheFactory;

    private readonly IOptionsMonitor<DiscoveryOptions> _options;

    private readonly IServiceEndPointProvider _serviceEndPointProvider;

    private readonly ConcurrentDictionary<string, SemaphoreSlim> _cacheLocks = new();

    private const string ServiceEndPointsKeyPrefix = "ServiceEndPoints:";

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="serviceEndPointProvider"></param>
    /// <param name="options"></param>
    /// <param name="cacheFactory"></param>
    public DiscoveryClient(IServiceEndPointProvider serviceEndPointProvider, IOptionsMonitor<DiscoveryOptions> options, ICacheFactory cacheFactory)
    {
        _serviceEndPointProvider = serviceEndPointProvider;
        _options = options;
        _cacheFactory = cacheFactory;
        ProviderName = (_serviceEndPointProvider as IServiceNameAware)?.Name;
    }

    /// <summary>
    ///     配置
    /// </summary>
    private DiscoveryOptions Options => _options?.CurrentValue;

    /// <summary>
    ///     服务名称
    /// </summary>
    public string ProviderName { get; }

    /// <summary>
    ///     获取所有实例
    /// </summary>
    /// <param name="group"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IList<IServiceEndPoint>> GetAllEndPointsAsync(string group = null, CancellationToken cancellationToken = default)
    {
        var allInstances = new List<IServiceEndPoint>();
        var serviceNames = await GetServicesAsync(group, cancellationToken);

        foreach (var serviceName in serviceNames)
        {
            var instances = await GetServiceEndPointsAsync(serviceName, group, cancellationToken: cancellationToken);
            allInstances.AddRange(instances);
        }

        return allInstances;
    }

    /// <summary>
    ///     获取服务实例
    /// </summary>
    /// <param name="serviceName"></param>
    /// <param name="group"></param>
    /// <param name="passingOnly"></param>
    /// <param name="tag"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IReadOnlyList<IServiceEndPoint>> GetServiceEndPointsAsync(string serviceName, string group = null, bool passingOnly = true, string tag = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ServiceEndPointsKeyPrefix}{serviceName}";

        if (!Options.Cache)
            return await _serviceEndPointProvider.GetServiceEndPointsAsync(serviceName, group, passingOnly, tag, cancellationToken).ConfigureAwait(false);

        var cache = _cacheFactory.Create(Options.CacheStrategy);

        var instanceData = await cache.GetAsync<IReadOnlyList<IServiceEndPoint>>(cacheKey, cancellationToken).ConfigureAwait(false);
        if (instanceData is { Count: > 0 })
            return instanceData;

        //  缓存未命中时按 key 串行化回源，避免缓存击穿；等待期间其他请求可能已完成回源
        var semaphore = _cacheLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            //  双检：等待期间已有其他请求写缓存则直接复用
            instanceData = await cache.GetAsync<IReadOnlyList<IServiceEndPoint>>(cacheKey, cancellationToken).ConfigureAwait(false);
            if (instanceData is { Count: > 0 })
                return instanceData;

            var instances = await _serviceEndPointProvider.GetServiceEndPointsAsync(serviceName, group, passingOnly, tag, cancellationToken).ConfigureAwait(false);

            //  空结果（服务缩容到 0 实例或 Consul 瞬时异常返回空）不写入缓存，避免故障窗口被 CacheTtl 放大
            if (instances is { Count: > 0 })
                await cache.AddAsync(cacheKey, instances, TimeSpan.FromSeconds(Options.CacheTtl), cancellationToken).ConfigureAwait(false);

            return instances;
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <summary>
    ///     获取服务实例
    /// </summary>
    /// <param name="group"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IEnumerable<string>> GetServicesAsync(string group = null, CancellationToken cancellationToken = default)
    {
        return _serviceEndPointProvider.GetServicesAsync(group, cancellationToken);
    }
}