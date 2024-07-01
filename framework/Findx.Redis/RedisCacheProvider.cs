using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.Caching;

namespace Findx.Redis;

public class RedisCacheProvider : ICache
{
    private readonly IRedisClient _redisClient;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="redisClientProvider"></param>
    public RedisCacheProvider(IRedisClientProvider redisClientProvider)
    {
        _redisClient = redisClientProvider.CreateClient();
        Name = CacheType.DefaultRedis;
    }

    public string Name { get; }

    public void Clear()
    {
        _redisClient.Clear();
    }

    public Task ClearAsync(CancellationToken token = default)
    {
        return _redisClient.ClearAsync();
    }

    public bool Exists(string key)
    {
        return _redisClient.Exists(key);
    }

    public Task<bool> ExistsAsync(string key, CancellationToken token = default)
    {
        return _redisClient.ExistsAsync(key);
    }

    public T Get<T>(string key)
    {
        return _redisClient.StringGet<T>(key);
    }
        
    public bool TryAdd<T>(string key, T value)
    {
        return _redisClient.StringSet(key, value, TimeSpan.MaxValue, whenNotExists: true);
    }

    public bool TryAdd<T>(string key, T value, TimeSpan absoluteExpiration)
    {
        return _redisClient.StringSet(key, value, absoluteExpiration, whenNotExists: true);
    }

    public bool TryAdd<T>(string key, T value, DateTime absoluteExpiration)
    {
        return _redisClient.StringSet(key, value, absoluteExpiration, whenNotExists: true);
    }

    public bool TryAdd<T>(string key, T value, SlidingExpirationOptions slidingExpirationOptions)
    {
        throw new NotImplementedException();
    }

    public void Add<T>(string key, T value)
    {
        _redisClient.StringSet(key, value, TimeSpan.MaxValue);
    }

    public void Add<T>(string key, T value, TimeSpan absoluteExpiration)
    {
        _redisClient.StringSet(key, value, absoluteExpiration);
    }

    public void Add<T>(string key, T value, DateTime absoluteExpiration)
    {
        _redisClient.StringSet(key, value, absoluteExpiration);
    }

    public void Add<T>(string key, T value, SlidingExpirationOptions slidingExpirationOptions)
    {
        throw new NotImplementedException();
    }

    public Task<T> GetAsync<T>(string key, CancellationToken token = default)
    {
        return _redisClient.StringGetAsync<T>(key, token);
    }

    public Task<bool> TryAddAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        return _redisClient.StringSetAsync(key, value, TimeSpan.MaxValue, whenNotExists: true, cancellationToken: cancellationToken);
    }

    public Task<bool> TryAddAsync<T>(string key, T value, TimeSpan absoluteExpiration, CancellationToken cancellationToken = default)
    {
        return _redisClient.StringSetAsync(key, value, absoluteExpiration, whenNotExists: true, cancellationToken: cancellationToken);
    }

    public Task<bool> TryAddAsync<T>(string key, T value, DateTime absoluteExpiration, CancellationToken cancellationToken = default)
    {
        return _redisClient.StringSetAsync(key, value, absoluteExpiration, whenNotExists: true, cancellationToken: cancellationToken);
    }

    public Task<bool> TryAddAsync<T>(string key, T value, SlidingExpirationOptions slidingExpirationOptions, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        return _redisClient.StringSetAsync(key, value, TimeSpan.MaxValue, cancellationToken: cancellationToken);
    }

    public Task AddAsync<T>(string key, T value, TimeSpan absoluteExpiration, CancellationToken cancellationToken = default)
    {
        return _redisClient.StringSetAsync(key, value, absoluteExpiration, cancellationToken: cancellationToken);
    }

    public Task AddAsync<T>(string key, T value, DateTime absoluteExpiration, CancellationToken cancellationToken = default)
    {
        return _redisClient.StringSetAsync(key, value, absoluteExpiration, cancellationToken: cancellationToken);
    }

    public Task AddAsync<T>(string key, T value, SlidingExpirationOptions slidingExpirationOptions, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Remove(string key)
    {
        _redisClient.Remove(key);
    }

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        return _redisClient.RemoveAsync(key);
    }

    public void RemoveByPrefix(string prefix)
    {
        var keys = _redisClient.SearchKeys($"{prefix}*");
        _redisClient.RemoveAll(keys);
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken token = default)
    {
        var keys = _redisClient.SearchKeys($"{prefix}*");
        return _redisClient.RemoveAllAsync(keys);
    }
}