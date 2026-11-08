using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Findx.Caching;
using Findx.Common;
using Findx.Extensions;

namespace Findx.Redis;

public class RedisCache : ICache
{
    /// <summary>
    ///     计数器 key 前缀：缓存值以 Hash 结构存储，计数器以 String 结构存储，
    ///     加前缀隔离数据结构，避免同一 key 触发 WRONGTYPE。
    /// </summary>
    private const string CounterKeyPrefix = "incr:";

    private readonly IRedisClient _redisClient;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="redisClientProvider"></param>
    public RedisCache(IRedisClientProvider redisClientProvider)
    {
        _redisClient = redisClientProvider.CreateClient();
        Name = CacheType.DefaultRedis;
    }

    public string Name { get; }

    public bool Exists(string key)
    {
        return _redisClient.Exists(key);
    }

    public Task<bool> ExistsAsync(string key, CancellationToken token = default)
    {
        return _redisClient.ExistsAsync(key, token);
    }

    public T Get<T>(string key)
    {
        var values = _redisClient.HGetAll(key);
        
        if (values.IsNullOrEmpty()) return default;
        
        if (!values.ContainsKey(key)) return default;
        
        if (values[key].IsNullOrWhiteSpace()) return default;
        
        var value = values[key];
        var type = typeof(T);
        var returnValue = type.IsPrimitiveExtendedIncludingNullable(true) ? value.CastTo<T>() : value.ToObject<T>();

        if (values.ContainsKey("SlidingExpiration"))
        {
            var slidingExpiration = TimeSpan.FromSeconds(values["SlidingExpiration"].CastTo<long>());
            _redisClient.Expire(key, slidingExpiration);
        }
        
        return returnValue;
    }
        
    public bool TryAdd<T>(string key, T value)
    {
        var valueString = typeof(T).IsPrimitiveExtendedIncludingNullable(true) ? value.ToString() : value.ToJson();
        return _redisClient.HSet(key, key, valueString, When.NotExists);
    }

    public bool TryAdd<T>(string key, T value, TimeSpan absoluteExpiration)
    {
        var valueString = typeof(T).IsPrimitiveExtendedIncludingNullable(true) ? value.ToString() : value.ToJson();
        var r = _redisClient.HSet(key, key, valueString, When.NotExists);
        if (r)
        {
            _redisClient.HDel(key, "SlidingExpiration");
            _redisClient.Expire(key, absoluteExpiration);
            return true;
        }
        return false;
    }

    public bool TryAdd<T>(string key, T value, DateTime absoluteExpiration)
    {
        var valueString = typeof(T).IsPrimitiveExtendedIncludingNullable(true) ? value.ToString() : value.ToJson();
        var r = _redisClient.HSet(key, key, valueString, When.NotExists);
        if (r)
        {
            _redisClient.HDel(key, "SlidingExpiration");
            _redisClient.Expire(key, absoluteExpiration.ToUniversalTime() - DateTime.UtcNow);
            return true;
        }
        return false;
    }

    public bool TryAdd<T>(string key, T value, SlidingExpirationOptions slidingExpirationOptions)
    {
        var valueString = typeof(T).IsPrimitiveExtendedIncludingNullable(true) ? value.ToString() : value.ToJson();
        var r = _redisClient.HSet(key, key, valueString, When.NotExists);
        if (r)
        {
            _redisClient.HSet(key, "SlidingExpiration", slidingExpirationOptions.SlidingExpiration.TotalSeconds.ToString(CultureInfo.InvariantCulture));
            _redisClient.Expire(key, slidingExpirationOptions.SlidingExpiration);
            return true;
        }
        return false;
    }

    public void Add<T>(string key, T value)
    {
        var valueString = typeof(T).IsPrimitiveExtendedIncludingNullable(true) ? value.ToString() : value.ToJson();
        _redisClient.HSet(key, key, valueString);
    }

    public void Add<T>(string key, T value, TimeSpan absoluteExpiration)
    {
        var valueString = typeof(T).IsPrimitiveExtendedIncludingNullable(true) ? value.ToString() : value.ToJson();
        _redisClient.HSet(key, key, valueString);
        _redisClient.HDel(key, "SlidingExpiration");
        _redisClient.Expire(key, absoluteExpiration);
    }

    public void Add<T>(string key, T value, DateTime absoluteExpiration)
    {
        var valueString = typeof(T).IsPrimitiveExtendedIncludingNullable(true) ? value.ToString() : value.ToJson();
        _redisClient.HSet(key, key, valueString);
        _redisClient.HDel(key, "SlidingExpiration");
        _redisClient.Expire(key, absoluteExpiration.ToUniversalTime() - DateTime.UtcNow);
    }

    public void Add<T>(string key, T value, SlidingExpirationOptions slidingExpirationOptions)
    {
        var valueString = typeof(T).IsPrimitiveExtendedIncludingNullable(true) ? value.ToString() : value.ToJson();
        _redisClient.HSet(key, key, valueString);
        _redisClient.HSet(key, "SlidingExpiration", slidingExpirationOptions.SlidingExpiration.TotalSeconds.ToString(CultureInfo.InvariantCulture));
        _redisClient.Expire(key, slidingExpirationOptions.SlidingExpiration);
    }

    public async Task<T> GetAsync<T>(string key, CancellationToken token = default)
    {
        var values = await _redisClient.HGetAllAsync(key, token);
        
        if (values.IsNullOrEmpty()) return default;
        
        if (!values.ContainsKey(key)) return default;
        
        if (values[key].IsNullOrWhiteSpace()) return default;
        
        var value = values[key];
        var type = typeof(T);
        var returnValue = type.IsPrimitiveExtendedIncludingNullable(true) ? value.CastTo<T>() : value.ToObject<T>();

        if (values.ContainsKey("SlidingExpiration"))
        {
            var slidingExpiration = TimeSpan.FromSeconds(values["SlidingExpiration"].CastTo<long>());
            await _redisClient.ExpireAsync(key, slidingExpiration, token);
        }
        
        return returnValue;
    }

    public async Task<bool> TryAddAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        var valueString = typeof(T).IsPrimitiveExtendedIncludingNullable(true) ? value.ToString() : value.ToJson();
        return await _redisClient.HSetAsync(key, key, valueString, When.NotExists, cancellationToken);
    }

    public async Task<bool> TryAddAsync<T>(string key, T value, TimeSpan absoluteExpiration, CancellationToken cancellationToken = default)
    {
        var valueString = typeof(T).IsPrimitiveExtendedIncludingNullable(true) ? value.ToString() : value.ToJson();
        var r = await _redisClient.HSetAsync(key, key, valueString, When.NotExists, cancellationToken);
        if (r)
        {
            await _redisClient.HDelAsync(key, "SlidingExpiration", cancellationToken);
            await _redisClient.ExpireAsync(key, absoluteExpiration, cancellationToken);
            return true;
        }
        return false;
    }

    public async Task<bool> TryAddAsync<T>(string key, T value, DateTime absoluteExpiration, CancellationToken cancellationToken = default)
    {
        var valueString = typeof(T).IsPrimitiveExtendedIncludingNullable(true) ? value.ToString() : value.ToJson();
        var r = await _redisClient.HSetAsync(key, key, valueString, When.NotExists, cancellationToken);
        if (r)
        {
            await _redisClient.HDelAsync(key, "SlidingExpiration", cancellationToken);
            await _redisClient.ExpireAsync(key, absoluteExpiration.ToUniversalTime() - DateTime.UtcNow, cancellationToken);
            return true;
        }
        return false;
    }

    public async Task<bool> TryAddAsync<T>(string key, T value, SlidingExpirationOptions slidingExpirationOptions, CancellationToken cancellationToken = default)
    {
        var valueString = typeof(T).IsPrimitiveExtendedIncludingNullable(true) ? value.ToString() : value.ToJson();
        var r = await _redisClient.HSetAsync(key, key, valueString, When.NotExists, cancellationToken);
        if (r)
        {
            await _redisClient.HSetAsync(key, "SlidingExpiration", slidingExpirationOptions.SlidingExpiration.TotalSeconds.ToString(CultureInfo.InvariantCulture), cancellationToken: cancellationToken);
            await _redisClient.ExpireAsync(key, slidingExpirationOptions.SlidingExpiration, cancellationToken);
            return true;
        }
        return false;
    }

    public async Task AddAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        var valueString = typeof(T).IsPrimitiveExtendedIncludingNullable(true) ? value.ToString() : value.ToJson();
        await _redisClient.HSetAsync(key, key, valueString, cancellationToken: cancellationToken);
    }

    public async Task AddAsync<T>(string key, T value, TimeSpan absoluteExpiration, CancellationToken cancellationToken = default)
    {
        var valueString = typeof(T).IsPrimitiveExtendedIncludingNullable(true) ? value.ToString() : value.ToJson();
        await _redisClient.HSetAsync(key, key, valueString, cancellationToken: cancellationToken);
        await _redisClient.HDelAsync(key, "SlidingExpiration", cancellationToken);
        await _redisClient.ExpireAsync(key, absoluteExpiration, cancellationToken);
    }

    public async Task AddAsync<T>(string key, T value, DateTime absoluteExpiration, CancellationToken cancellationToken = default)
    {
        var valueString = typeof(T).IsPrimitiveExtendedIncludingNullable(true) ? value.ToString() : value.ToJson();
        await _redisClient.HSetAsync(key, key, valueString, cancellationToken: cancellationToken);
        await _redisClient.HDelAsync(key, "SlidingExpiration", cancellationToken);
        await _redisClient.ExpireAsync(key, absoluteExpiration.ToUniversalTime() - DateTime.UtcNow, cancellationToken);
    }

    public async Task AddAsync<T>(string key, T value, SlidingExpirationOptions slidingExpirationOptions, CancellationToken cancellationToken = default)
    {
        var valueString = typeof(T).IsPrimitiveExtendedIncludingNullable(true) ? value.ToString() : value.ToJson();
        await _redisClient.HSetAsync(key, key, valueString, cancellationToken: cancellationToken);
        await _redisClient.HSetAsync(key, "SlidingExpiration", slidingExpirationOptions.SlidingExpiration.TotalSeconds.ToString(CultureInfo.InvariantCulture), cancellationToken: cancellationToken);
        await _redisClient.ExpireAsync(key, slidingExpirationOptions.SlidingExpiration, cancellationToken);
    }

    public void Remove(string key)
    {
        _redisClient.Remove(key);
    }

    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
        await _redisClient.RemoveAsync(key, token);
    }
    
    public long Increment(string key, long value = 1, TimeSpan? absoluteExpiration = null)
    {
        var cacheKey = CounterKeyPrefix + key;
        var r = _redisClient.IncrBy(cacheKey, value);
        if (absoluteExpiration.HasValue) _redisClient.Expire(cacheKey, absoluteExpiration.Value);
        return r;
    }

    public long Decrement(string key, long value = 1, TimeSpan? absoluteExpiration = null)
    {
        var cacheKey = CounterKeyPrefix + key;
        var r = _redisClient.DecrBy(cacheKey, value);
        if (absoluteExpiration.HasValue) _redisClient.Expire(cacheKey, absoluteExpiration.Value);
        return r;
    }

    public async Task<long> IncrementAsync(string key, long value = 1, TimeSpan? absoluteExpiration = null, CancellationToken token = default)
    {
        var cacheKey = CounterKeyPrefix + key;
        var r = await _redisClient.IncrByAsync(cacheKey, value, token);
        if (absoluteExpiration.HasValue) await _redisClient.ExpireAsync(cacheKey, absoluteExpiration.Value, token);
        return r;
    }

    public async Task<long> DecrementAsync(string key, long value = 1, TimeSpan? absoluteExpiration = null, CancellationToken token = default)
    {
        var cacheKey = CounterKeyPrefix + key;
        var r = await _redisClient.DecrByAsync(cacheKey, value, token);
        if (absoluteExpiration.HasValue) await _redisClient.ExpireAsync(cacheKey, absoluteExpiration.Value, token);
        return r;
    }
}