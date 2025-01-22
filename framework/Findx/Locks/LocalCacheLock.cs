using System.Threading.Tasks;
using Findx.Caching;
using Findx.DependencyInjection;
using Findx.Extensions;

namespace Findx.Locks;

/// <summary>
///     本地缓存锁
/// </summary>
public class LocalCacheLock : LockBase, IServiceNameAware
{
    private readonly ICache _cache;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="cacheFactory">缓存服务提供器</param>
    public LocalCacheLock(ICacheFactory cacheFactory)
    {
        _cache = cacheFactory.Create(CacheType.DefaultMemory);
    }

    /// <summary>
    /// 服务名称
    /// </summary>
    public string Name => "LocalCacheLock";

    /// <summary>
    ///     释放
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="lockId"></param>
    /// <param name="cancellationToken"></param>
    public override async Task ReleaseAsync(string resource, string lockId, CancellationToken cancellationToken = default)
    {
        var v = await _cache.GetAsync<string>(GenerateNewLockKey(resource), cancellationToken);
        
        if (v.IsNullOrWhiteSpace() || v != lockId) return;

        await _cache.RemoveAsync(GenerateNewLockKey(resource), cancellationToken);
    }

    /// <summary>
    ///     续期
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="lockId"></param>
    /// <param name="timeUntilExpires"></param>
    /// <param name="cancellationToken"></param>
    public override async Task RenewAsync(string resource, string lockId, TimeSpan timeUntilExpires, CancellationToken cancellationToken = default)
    {
        var v = await _cache.GetAsync<string>(GenerateNewLockKey(resource), cancellationToken);
        
        if (v.IsNotNullOrWhiteSpace() && v == lockId)
        {
            await _cache.AddAsync(GenerateNewLockKey(resource), lockId, timeUntilExpires, cancellationToken);
        }
    }

    /// <summary>
    ///     尝试拿锁
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="lockId"></param>
    /// <param name="timeUntilExpires"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task<bool> TryLockAsync(string resource, string lockId, TimeSpan timeUntilExpires, CancellationToken cancellationToken = default)
    {
        return await _cache.TryAddAsync(GenerateNewLockKey(resource), lockId, timeUntilExpires, cancellationToken);
    }
}