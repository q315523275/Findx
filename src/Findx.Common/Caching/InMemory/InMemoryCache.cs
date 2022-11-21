using System.Threading.Tasks;
using Findx.Threading;

namespace Findx.Caching.InMemory
{
    /// <summary>
    /// 内存缓存
    /// </summary>
    public class InMemoryCache : ICache, IDisposable
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name => CacheType.DefaultMemory;

        private readonly ConcurrentDictionary<string, CacheItem> _cache;

        private FindxAsyncTimer Timer { get; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="timer"></param>
        public InMemoryCache(FindxAsyncTimer timer)
        {
            _cache = new ConcurrentDictionary<string, CacheItem>();

            Timer = timer;
            Timer.Period = 1000 * 120; // 60 sec.
            Timer.Elapsed = Timer_Elapsed;
            Timer.RunOnStart = false;
            Timer.Start();
        }

        /// <summary>
        /// 调度执行方法
        /// </summary>
        /// <param name="timer"></param>
        private async Task Timer_Elapsed(FindxAsyncTimer timer)
        {
            var now = DateTime.Now;
            foreach (var item in _cache)
            {
                if (item.Value.ExpiredTime < now)
                    Remove(item.Key);
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 判断缓存是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<bool> ExistsAsync(string key, CancellationToken token = default)
        {
            Check.NotNull(key, nameof(key));

            if (!_cache.TryGetValue(key, out var item))
            {
                return Task.FromResult(false);
            }

            // 存在且过期,删除缓存
            if (item.Expired)
            {
                _cache.Remove(key, out var _);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// 获取缓存值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="expire"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string key, Func<Task<T>> func, TimeSpan? expire = null, CancellationToken token = default)
        {
            Check.NotNull(key, nameof(key));

            if (_cache.TryGetValue(key, out var item) && !item.Expired)
            {
                return (T)item.Visit();
            }

            var value = await func.Invoke();

            Add(key, value, expire);

            return value;
        }

        /// <summary>
        /// 获取缓存值
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
                return default;
            }

            // 存在且过期,删除缓存
            if (item.Expired)
            {
                _cache.Remove(key, out var _);
                return default;
            }

            await Task.CompletedTask;

            return (T)item.Visit();
        }

        /// <summary>
        /// 添加缓存,已存在则添加失败
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<bool> TryAddAsync<T>(string key, T value, TimeSpan? expire = null, CancellationToken token = default)
        {
            Check.NotNull(key, nameof(key));

            // 存在且未过期
            if (_cache.TryGetValue(key, out var item) && !item.Expired)
                return Task.FromResult(false);

            Add(key, value, expire);

            return Task.FromResult(true);
        }

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task AddAsync<T>(string key, T value, TimeSpan? expire = null, CancellationToken token = default)
        {
            Check.NotNull(key, nameof(key));

            _cache.AddOrUpdate(key, new CacheItem(value, expire), (_, oldItem) =>
            {
                oldItem.Set(value, expire);
                return oldItem;
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// 移除缓存
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
        /// 根据前缀移除缓存
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task RemoveByPrefixAsync(string prefix, CancellationToken token = default)
        {
            foreach (var item in _cache)
            {
                if (item.Key.StartsWith(prefix))
                    Remove(item.Key);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task ClearAsync(CancellationToken token = default)
        {
            _cache.Clear();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 判断缓存是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            Check.NotNull(key, nameof(key));

            if (!_cache.TryGetValue(key, out var item))
            {
                return false;
            }

            // 存在且过期,删除缓存
            if (item.Expired)
            {
                _cache.Remove(key, out var _);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取缓存值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        public T Get<T>(string key, Func<T> func, TimeSpan? expire = null)
        {
            Check.NotNull(key, nameof(key));

            if (_cache.TryGetValue(key, out var item) && !item.Expired)
            {
                return (T)item.Visit();
            }

            var value = func.Invoke();

            Add(key, value, expire);

            return value;
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            Check.NotNull(key, nameof(key));

            if (!_cache.TryGetValue(key, out var item))
            {
                return default;
            }

            // 存在且过期,删除缓存
            if (item.Expired)
            {
                _cache.Remove(key, out var _);
                return default;
            }

            return (T)item.Visit();
        }

        /// <summary>
        /// 添加缓存,已存在则添加失败
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        public bool TryAdd<T>(string key, T value, TimeSpan? expire = null)
        {
            Check.NotNull(key, nameof(key));

            if (_cache.TryGetValue(key, out var item) && !item.Expired)
            {
                return false;
            }

            Add(key, value, expire);

            return true;
        }

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        public void Add<T>(string key, T value, TimeSpan? expire = null)
        {
            Check.NotNull(key, nameof(key));

            _cache.AddOrUpdate(key, new CacheItem(value, expire), (_, oldItem) =>
            {
                oldItem.Set(value, expire);
                return oldItem;
            });
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            _cache.TryRemove(key, out _);
        }

        /// <summary>
        /// 根据缓存前缀移除缓存
        /// </summary>
        /// <param name="prefix"></param>
        public void RemoveByPrefix(string prefix)
        {
            foreach (var item in _cache)
            {
                if (item.Key.StartsWith(prefix))
                    Remove(item.Key);
            }
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            _cache?.Clear();

            Timer.Stop();
        }
    }
    internal class CacheItem
    {
        /// <summary>
        /// 数值
        /// </summary>
        private object Value { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpiredTime { get; private set; }

        /// <summary>
        /// 是否过期
        /// </summary>
        public bool Expired => ExpiredTime <= DateTime.Now;

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

            var now = DateTime.Now;
            ExpiredTime = expire == null ? DateTime.MaxValue : now.AddSeconds(expire.Value.TotalSeconds);
        }

        /// <summary>
        /// 更新访问时间并返回数值
        /// </summary>
        /// <returns></returns>
        public object Visit()
        {
            return Value;
        }
    }
}
