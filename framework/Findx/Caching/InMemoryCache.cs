using System.Threading.Tasks;
using Findx.Common;
using Findx.Common.Atomic;
using Findx.Threading;

namespace Findx.Caching;

/// <summary>
///     内存缓存
/// </summary>
public class InMemoryCache : ICache, IDisposable
{
    /// <summary>
    ///     存储字典
    /// </summary>
    private readonly ConcurrentDictionary<string, CacheItem> _cache;

    /// <summary>
    ///     命中次数
    /// </summary>
    private long _hits;

    /// <summary>
    ///     丢失次数
    /// </summary>
    private long _misses;

    /// <summary>
    ///     写入次数
    /// </summary>
    private long _writes;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="timer"></param>
    public InMemoryCache(AsyncTimer timer)
    {
        _cache = new ConcurrentDictionary<string, CacheItem>();

        Timer = timer;
        Timer.Period = 1000 * 100; // 120 sec.
        Timer.Elapsed = Timer_Elapsed;
        Timer.RunOnStart = false;
        Timer.Start();
    }

    /// <summary>
    ///     定时器
    /// </summary>
    private AsyncTimer Timer { get; }

    /// <summary>
    ///     缓存总数
    /// </summary>
    public int Count => _cache.Count;

    /// <summary>
    ///     交互次数
    /// </summary>
    public long Calls => _writes + _hits + _misses;

    /// <summary>
    ///     写入次数
    /// </summary>
    public long Writes => _writes;

    /// <summary>
    ///     读取次数
    /// </summary>
    public long Reads => _hits + _misses;

    /// <summary>
    ///     缓存命中次数
    /// </summary>
    public long Hits => _hits;

    /// <summary>
    ///     丢失次数
    /// </summary>
    public long Misses => _misses;

    /// <summary>
    ///     名称
    /// </summary>
    public string Name => CacheType.DefaultMemory;
    
    /// <summary>
    ///     判断缓存是否存在
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Exists(string key)
    {
        Check.NotNull(key, nameof(key));

        if (!_cache.TryGetValue(key, out var item)) return false;

        // 存在未过期
        if (!item.Expired) return true;
        
        // 存在且过期,删除缓存
        _cache.Remove(key, out _);
        return false;
    }
    
    /// <summary>
    ///     判断缓存是否存在
    /// </summary>
    /// <param name="key"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<bool> ExistsAsync(string key, CancellationToken token = default)
    {
        Check.NotNull(key, nameof(key));

        if (!_cache.TryGetValue(key, out var item))
            return Task.FromResult(false);

        // 未过期
        if (!item.Expired) 
            return Task.FromResult(true);
        
        // 过期,删除缓存
        _cache.Remove(key, out _);
        return Task.FromResult(false);
    }

    /// <summary>
    ///     获取缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T>(string key)
    {
        Check.NotNull(key, nameof(key));

        if (!_cache.TryGetValue(key, out var item))
        {
            Interlocked.Increment(ref _misses);
            return default;
        }

        // 存在且过期,删除缓存
        if (item.Expired)
        {
            Interlocked.Increment(ref _misses);
            _cache.Remove(key, out _);
            return default;
        }

        Interlocked.Increment(ref _hits);

        return (T)item.Visit();
    }
    
    /// <summary>
    ///     获取缓存值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<T> GetAsync<T>(string key, CancellationToken token = default)
    {
        Check.NotNull(key, nameof(key));

        if (!_cache.TryGetValue(key, out var item))
        {
            Interlocked.Increment(ref _misses);
            return default;
        }

        // 存在且过期,删除缓存
        if (item.Expired)
        {
            Interlocked.Increment(ref _misses);
            _cache.Remove(key, out _);
            return default;
        }

        Interlocked.Increment(ref _hits);
        
        return await Task.FromResult((T)item.Visit());
    }
    
    /// <summary>
    ///     当缓存健不存在时则添加缓存信息
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool TryAdd<T>(string key, T value)
    {
        Check.NotNull(key, nameof(key));

        if (_cache.TryGetValue(key, out var item) && !item.Expired)
            return false;

        _cache.Remove(key, out var cacheValue);
        cacheValue?.Set(value);
        cacheValue ??= new CacheItem(value);
        
        if (_cache.TryAdd(key, cacheValue))
        {
            Interlocked.Increment(ref _writes);
            return true;
        }
        
        return false;
    }

    /// <summary>
    ///     当缓存健不存在时则添加缓存信息
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expire"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool TryAdd<T>(string key, T value, TimeSpan expire)
    {
        Check.NotNull(key, nameof(key));
        
        if (_cache.TryGetValue(key, out var item) && !item.Expired)
            return false;

        _cache.Remove(key, out var cacheValue);
        cacheValue?.Set(value, expire);
        cacheValue ??= new CacheItem(value, expire);
        
        if (_cache.TryAdd(key, cacheValue))
        {
            Interlocked.Increment(ref _writes);
            return true;
        }
        
        return false;
    }

    /// <summary>
    ///     当缓存健不存在时则添加缓存信息
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expire"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool TryAdd<T>(string key, T value, DateTime expire)
    {
        Check.NotNull(key, nameof(key));

        if (_cache.TryGetValue(key, out var item) && !item.Expired)
            return false;

        _cache.Remove(key, out var cacheValue);
        cacheValue?.Set(value, expire);
        cacheValue ??= new CacheItem(value, expire);
        
        if (_cache.TryAdd(key, cacheValue))
        {
            Interlocked.Increment(ref _writes);
            return true;
        }
        
        return false;
    }

    /// <summary>
    ///     当缓存健不存在时则添加缓存信息
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="slidingExpirationOptions"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool TryAdd<T>(string key, T value, SlidingExpirationOptions slidingExpirationOptions)
    {
        Check.NotNull(key, nameof(key));

        if (_cache.TryGetValue(key, out var item) && !item.Expired)
            return false;

        _cache.Remove(key, out var cacheValue);
        cacheValue?.Set(value, slidingExpirationOptions);
        cacheValue ??= new CacheItem(value, slidingExpirationOptions);
        
        if (_cache.TryAdd(key, cacheValue))
        {
            Interlocked.Increment(ref _writes);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    ///     当缓存健不存在时则添加缓存信息
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task<bool> TryAddAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        Check.NotNull(key, nameof(key));

        if (_cache.TryGetValue(key, out var item) && !item.Expired)
            return Task.FromResult(false);

        _cache.Remove(key, out var cacheValue);
        cacheValue?.Set(value);
        cacheValue ??= new CacheItem(value);
        
        if (_cache.TryAdd(key, cacheValue))
        {
            Interlocked.Increment(ref _writes);
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }

    /// <summary>
    ///     当缓存健不存在时则添加缓存信息
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expire"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task<bool> TryAddAsync<T>(string key, T value, TimeSpan expire, CancellationToken cancellationToken = default)
    {
        Check.NotNull(key, nameof(key));

        if (_cache.TryGetValue(key, out var item) && !item.Expired)
            return Task.FromResult(false);

        _cache.Remove(key, out var cacheValue);
        cacheValue?.Set(value, expire);
        cacheValue ??= new CacheItem(value, expire);
        
        if (_cache.TryAdd(key, cacheValue))
        {
            Interlocked.Increment(ref _writes);
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }

    /// <summary>
    ///     当缓存健不存在时则添加缓存信息
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expire"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task<bool> TryAddAsync<T>(string key, T value, DateTime expire, CancellationToken cancellationToken = default)
    {
        Check.NotNull(key, nameof(key));
        
        if (_cache.TryGetValue(key, out var item) && !item.Expired)
            return Task.FromResult(false);

        _cache.Remove(key, out var cacheValue);
        cacheValue?.Set(value, expire);
        cacheValue ??= new CacheItem(value, expire);
        
        if (_cache.TryAdd(key, cacheValue))
        {
            Interlocked.Increment(ref _writes);
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }

    /// <summary>
    ///     当缓存健不存在时则添加缓存信息
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="slidingExpirationOptions"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task<bool> TryAddAsync<T>(string key, T value, SlidingExpirationOptions slidingExpirationOptions, CancellationToken cancellationToken = default)
    {
        Check.NotNull(key, nameof(key));
        
        if (_cache.TryGetValue(key, out var item) && !item.Expired)
            return Task.FromResult(false);

        _cache.Remove(key, out var cacheValue);
        cacheValue?.Set(value, slidingExpirationOptions);
        cacheValue ??= new CacheItem(value, slidingExpirationOptions);
        
        if (_cache.TryAdd(key, cacheValue))
        {
            Interlocked.Increment(ref _writes);
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }

        
    /// <summary>
    ///     添加缓存键和缓存值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    public void Add<T>(string key, T value)
    {
        Check.NotNull(key, nameof(key));

        Interlocked.Increment(ref _writes);

        _cache.AddOrUpdate(key, new CacheItem(value), (_, oldItem) => { oldItem.Set(value); return oldItem; });
    }

    /// <summary>
    ///     添加缓存键和缓存值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expire"></param>
    /// <typeparam name="T"></typeparam>
    public void Add<T>(string key, T value, TimeSpan expire)
    {
        Check.NotNull(key, nameof(key));

        Interlocked.Increment(ref _writes);

        _cache.AddOrUpdate(key, new CacheItem(value, expire), (_, oldItem) => { oldItem.Set(value, expire); return oldItem; });
    }

    /// <summary>
    ///     添加缓存键和缓存值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expire"></param>
    /// <typeparam name="T"></typeparam>
    public void Add<T>(string key, T value, DateTime expire)
    {
        Check.NotNull(key, nameof(key));

        Interlocked.Increment(ref _writes);

        _cache.AddOrUpdate(key, new CacheItem(value, expire), (_, oldItem) => { oldItem.Set(value, expire); return oldItem; });
    }

    /// <summary>
    ///     添加缓存键和缓存值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="slidingExpirationOptions"></param>
    /// <typeparam name="T"></typeparam>
    public void Add<T>(string key, T value, SlidingExpirationOptions slidingExpirationOptions)
    {
        Check.NotNull(key, nameof(key));

        Interlocked.Increment(ref _writes);

        _cache.AddOrUpdate(key, new CacheItem(value, slidingExpirationOptions), (_, oldItem) => { oldItem.Set(value, slidingExpirationOptions); return oldItem; });
    }
    
    /// <summary>
    ///     添加缓存键和缓存值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task AddAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        Check.NotNull(key, nameof(key));

        Interlocked.Increment(ref _writes);

        _cache.AddOrUpdate(key, new CacheItem(value), (_, oldItem) => { oldItem.Set(value); return oldItem; });
        
        return Task.CompletedTask;
    }

    /// <summary>
    ///     添加缓存键和缓存值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expire"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task AddAsync<T>(string key, T value, TimeSpan expire, CancellationToken cancellationToken = default)
    {
        Check.NotNull(key, nameof(key));

        Interlocked.Increment(ref _writes);

        _cache.AddOrUpdate(key, new CacheItem(value, expire), (_, oldItem) => { oldItem.Set(value, expire); return oldItem; });
        
        return Task.CompletedTask;
    }

    /// <summary>
    ///     添加缓存键和缓存值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expire"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task AddAsync<T>(string key, T value, DateTime expire, CancellationToken cancellationToken = default)
    {
        Check.NotNull(key, nameof(key));

        Interlocked.Increment(ref _writes);

        _cache.AddOrUpdate(key, new CacheItem(value, expire), (_, oldItem) => { oldItem.Set(value, expire); return oldItem; });
        
        return Task.CompletedTask;
    }

    /// <summary>
    ///     添加缓存键和缓存值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="slidingExpirationOptions"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task AddAsync<T>(string key, T value, SlidingExpirationOptions slidingExpirationOptions, CancellationToken cancellationToken = default)
    {
        Check.NotNull(key, nameof(key));

        Interlocked.Increment(ref _writes);

        _cache.AddOrUpdate(key, new CacheItem(value, slidingExpirationOptions), (_, oldItem) => { oldItem.Set(value, slidingExpirationOptions); return oldItem; });
        
        return Task.CompletedTask;
    }

    /// <summary>
    ///     移除缓存
    /// </summary>
    /// <param name="key"></param>
    public void Remove(string key)
    {
        _cache.TryRemove(key, out _);
    }
    
    /// <summary>
    ///     移除缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        Check.NotNull(key, nameof(key));

        _cache.TryRemove(key, out _);

        return Task.CompletedTask;
    }

    /// <summary>
    ///     移除缓存
    /// </summary>
    /// <param name="prefix"></param>
    public void RemoveByPrefix(string prefix)
    {
        foreach (var item in _cache)
        {
            if (item.Key.StartsWith(prefix))
            {
                _cache.TryRemove(item.Key, out _);
            }
        }
    }
    
    /// <summary>
    ///     获取 或 添加 缓存项
    /// </summary>
    /// <param name="key"></param>
    /// <param name="valueFactory"></param>
    /// <param name="expire"></param>
    /// <returns></returns>
    private CacheItem GetOrAddItem(string key, Func<string, object> valueFactory, TimeSpan? expire)
    {
        CacheItem item;
        do
        {
            if (_cache.TryGetValue(key, out item) && item != null)
            {
                if (!item.Expired) return item;

                if (expire.HasValue)
                    item.Set(valueFactory(key), expire.Value);
                else
                    item.Set(valueFactory(key));
                
                return item;
            }

            item = expire.HasValue ? new CacheItem(valueFactory(key), expire.Value) : new CacheItem(valueFactory(key));
            
        } while (!_cache.TryAdd(key, item));

        Interlocked.Increment(ref _writes);
        
        return item;
    }
    
    /// <summary>
    ///     累加，原子操作
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expire"></param>
    /// <returns></returns>
    public long Increment(string key, long value = 1, TimeSpan? expire = null)
    {
        var item = GetOrAddItem(key, _ => new AtomicLong(), expire);
        var atomicLong = item.Visit() as AtomicLong;
        return atomicLong?.AddAndGet(value)?? 0;
    }

    /// <summary>
    ///     递减，原子操作
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expire"></param>
    /// <returns></returns>
    public long Decrement(string key, long value = 1, TimeSpan? expire = null)
    {
        var item = GetOrAddItem(key, _ => new AtomicLong(), expire);
        var atomicLong = item.Visit() as AtomicLong;
        return atomicLong?.AddAndGet(-value)?? 0;
    }

    /// <summary>
    ///     累加，原子操作
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expire"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<long> IncrementAsync(string key, long value = 1, TimeSpan? expire = null, CancellationToken token = default)
    {
        var item = GetOrAddItem(key, _ => new AtomicLong(), expire);
        var atomicLong = item.Visit() as AtomicLong;
        return Task.FromResult(atomicLong?.AddAndGet(value)?? 0);
    }

    /// <summary>
    ///     递减，原子操作
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expire"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<long> DecrementAsync(string key, long value = 1, TimeSpan? expire = null, CancellationToken token = default)
    {
        var item = GetOrAddItem(key, _ => new AtomicLong(), expire);
        var atomicLong = item.Visit() as AtomicLong;
        return Task.FromResult(atomicLong?.AddAndGet(-value)?? 0);
    }

    /// <summary>
    ///     释放
    /// </summary>
    public void Dispose()
    {
        _cache?.Clear();
        Timer.Stop();
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"Count: {Count} Calls: {Calls} Reads: {Reads} Writes: {Writes} Hits: {Hits} Misses: {Misses}";
    }

    /// <summary>
    ///     重置统计
    /// </summary>
    public void ResetStats()
    {
        _writes = 0;
        _hits = 0;
        _misses = 0;
    }

    /// <summary>
    ///     调度执行方法
    /// </summary>
    /// <param name="timer"></param>
    private async Task Timer_Elapsed(AsyncTimer timer)
    {
        var now = DateTime.Now;
        foreach (var item in _cache)
            if (item.Value.ExpiredTime < now)
                _cache.TryRemove(item.Key, out _);
        await Task.CompletedTask;
    }
}

internal class CacheItem
{
    /// <summary>
    ///     构造缓存项
    /// </summary>
    /// <param name="value"></param>
    public CacheItem(object value)
    {
        Set(value);
    }
    
    /// <summary>
    ///     构造缓存项
    /// </summary>
    /// <param name="value"></param>
    /// <param name="expire"></param>
    public CacheItem(object value, TimeSpan expire)
    {
        Set(value, expire);
    }
    
    /// <summary>
    ///     构造缓存项
    /// </summary>
    /// <param name="value"></param>
    /// <param name="expire"></param>
    public CacheItem(object value, DateTime expire)
    {
        Set(value, expire);
    }
    
    /// <summary>
    ///     构造缓存项
    /// </summary>
    /// <param name="value"></param>
    /// <param name="slidingExpirationOptions"></param>
    public CacheItem(object value, SlidingExpirationOptions slidingExpirationOptions)
    {
        Set(value, slidingExpirationOptions);
    }

    /// <summary>
    ///     数值
    /// </summary>
    private object Value { get; set; }

    /// <summary>
    ///     滑动过期时间间隔
    /// </summary>
    private TimeSpan? SlidingExpiration { get; set; }

    /// <summary>
    ///     过期时间
    /// </summary>
    public DateTime ExpiredTime { get; private set; }

    /// <summary>
    ///     是否过期
    /// </summary>
    public bool Expired => ExpiredTime <= DateTime.Now;

    /// <summary>
    ///     刷新滑动过期时间
    /// </summary>
    public void RefreshSlidingTtl()
    {
        if (SlidingExpiration.HasValue)
        {
            ExpiredTime = DateTime.Now.AddSeconds(SlidingExpiration.Value.TotalSeconds);
        }
    }

    /// <summary>
    ///     更新访问时间并返回数值
    /// </summary>
    /// <returns></returns>
    public object Visit()
    {
        if (!Expired) RefreshSlidingTtl();
        return Value;
    }
    
    /// <summary>
    ///     设置数值和过期时间
    /// </summary>
    /// <param name="value"></param>
    public void Set(object value)
    {
        Value = value;
        ExpiredTime = DateTime.MaxValue;
    }
    
    /// <summary>
    ///     设置数值和过期时间
    /// </summary>
    /// <param name="value"></param>
    /// <param name="expire"></param>
    public void Set(object value, TimeSpan expire)
    {
        Value = value;
        ExpiredTime = DateTime.Now.AddSeconds(expire.TotalSeconds);
    }
    
    /// <summary>
    ///     设置数值和过期时间
    /// </summary>
    /// <param name="value"></param>
    /// <param name="expire"></param>
    public void Set(object value, DateTime expire)
    {
        Value = value;
        ExpiredTime = expire;
    }
    
    /// <summary>
    ///     设置数值和过期时间
    /// </summary>
    /// <param name="value"></param>
    /// <param name="slidingExpirationOptions"></param>
    public void Set(object value, SlidingExpirationOptions slidingExpirationOptions)
    {
        Value = value;
        ExpiredTime = DateTime.Now.AddSeconds(slidingExpirationOptions.SlidingExpiration.TotalSeconds);
        SlidingExpiration = slidingExpirationOptions.SlidingExpiration;
    }
}