using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Caching.InMemory
{
    public class InMemoryCache : ICache, IDisposable
    {
        public CacheType Name => CacheType.Memory;

        private readonly ConcurrentDictionary<string, CacheItem> _cache;
        private int _period = 60;
        private Timer _taskTimer;
        private bool _polling;

        public InMemoryCache()
        {
            _cache = new ConcurrentDictionary<string, CacheItem>();
            _taskTimer = new Timer(async x => { await RemoveNotAliveAsync(x); }, this, _period * 1000, _period * 1000);
        }

        public Task<bool> ExistsAsync(string key, CancellationToken token = default)
        {
            Check.NotNull(key, nameof(key));

            if (!_cache.TryGetValue(key, out var item) || item.Expired)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public async Task<T> GetAsync<T>(string key, Func<Task<T>> func, TimeSpan? expire = null, CancellationToken token = default)
        {
            Check.NotNull(key, nameof(key));

            if (_cache.TryGetValue(key, out var item) && !item.Expired)
                return (T)item.Visit();

            var value = await func.Invoke();

            Add(key, value, expire);

            return value;
        }

        public async Task<T> GetAsync<T>(string key, CancellationToken token = default)
        {
            Check.NotNull(key, nameof(key));

            if (!_cache.TryGetValue(key, out var item) || item.Expired)
                return default;

            await Task.CompletedTask;

            return (T)item.Visit();
        }

        public Task<bool> TryAddAsync<T>(string key, T value, TimeSpan? expire = null, CancellationToken token = default)
        {
            Check.NotNull(key, nameof(key));

            if (_cache.TryGetValue(key, out var item) && !item.Expired)
                return Task.FromResult(false);

            Add(key, value, expire);

            return Task.FromResult(true);
        }

        public Task AddAsync<T>(string key, T value, TimeSpan? expire = null, CancellationToken token = default)
        {
            Check.NotNull(key, nameof(key));

            _cache[key] = new CacheItem(value, expire);

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Check.NotNull(key, nameof(key));

            _cache.TryRemove(key, out var _);

            return Task.CompletedTask;
        }

        public Task RemoveByPrefixAsync(string prefix, CancellationToken token = default)
        {
            var keys = _cache.Keys.Where(it => it.StartsWith(prefix));
            foreach (var key in keys)
            {
                Remove(key);
            }
            return Task.CompletedTask;
        }

        public Task ClearAsync(CancellationToken token = default)
        {
            _cache.Clear();
            return Task.CompletedTask;
        }

        public bool Exists(string key)
        {
            Check.NotNull(key, nameof(key));

            if (!_cache.TryGetValue(key, out var item) || item.Expired)
                return false;

            return true;
        }

        public T Get<T>(string key, Func<T> func, TimeSpan? expire = null)
        {
            Check.NotNull(key, nameof(key));

            if (_cache.TryGetValue(key, out var item) && !item.Expired)
                return (T)item.Visit();

            var value = func.Invoke();

            Add(key, value, expire);

            return value;
        }

        public T Get<T>(string key)
        {
            Check.NotNull(key, nameof(key));

            if (!_cache.TryGetValue(key, out var item) || item.Expired)
                return default;

            return (T)item.Visit();
        }

        public bool TryAdd<T>(string key, T value, TimeSpan? expire = null)
        {
            Check.NotNull(key, nameof(key));

            if (_cache.TryGetValue(key, out var item) && !item.Expired)
                return false;

            Add(key, value, expire);

            return true;
        }

        public void Add<T>(string key, T value, TimeSpan? expire = null)
        {
            Check.NotNull(key, nameof(key));

            _cache[key] = new CacheItem(value, expire);
        }

        public void Remove(string key)
        {
            _cache.TryRemove(key, out var _);
        }

        public void RemoveByPrefix(string prefix)
        {
            var keys = _cache.Keys.Where(it => it.StartsWith(prefix));
            foreach (var key in keys)
            {
                Remove(key);
            }
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public void Dispose()
        {
            _cache?.Clear();

            _taskTimer?.Dispose();
            _taskTimer = null;
        }

        public Task RemoveNotAliveAsync(object state)
        {
            if (_polling)
            {
                return Task.CompletedTask;
            }
            _polling = true;

            InMemoryCache memoryCache = (InMemoryCache)state;
            var now = DateTime.Now;
            foreach (var item in memoryCache._cache)
            {
                if (item.Value.ExpiredTime < now)
                    Remove(item.Key);
            }
            _polling = false;

            return Task.CompletedTask;
        }
    }
    internal class CacheItem
    {
        private object _Value;

        /// <summary>
        /// 数值
        /// </summary>
        public object Value { get => _Value; set => _Value = value; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpiredTime { get; set; }

        /// <summary>
        /// 是否过期
        /// </summary>
        public bool Expired => ExpiredTime <= DateTime.Now;

        /// <summary>
        /// 访问时间
        /// </summary>
        public DateTime VisitTime { get; private set; }

        /// <summary>
        /// 构造缓存项
        /// </summary>
        /// <param name="value"></param>
        /// <param name="expire"></param>

        public CacheItem(object value, TimeSpan? expire = null) => Set(value, expire);

        /// <summary>
        /// 设置数值和过期时间
        /// </summary>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        public void Set(object value, TimeSpan? expire = null)
        {
            Value = value;

            var now = VisitTime = DateTime.Now;
            if (expire == null)
                ExpiredTime = DateTime.MaxValue;
            else
                ExpiredTime = now.AddSeconds(expire.Value.TotalSeconds);
        }

        /// <summary>
        /// 更新访问时间并返回数值
        /// </summary>
        /// <returns></returns>
        public object Visit()
        {
            VisitTime = DateTime.Now;
            return Value;
        }
    }
}
