using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.Caching;

namespace Findx.Redis
{
    public class RedisCacheProvider : ICache
    {
        private readonly IRedisClient _redisClient;

        public RedisCacheProvider(IRedisClientProvider redisClientProvider)
        {
            _redisClient = redisClientProvider.CreateClient();
            Name = $"Redis.{_redisClient.Name}";
        }

        public string Name { get; }

        public void Add<T>(string key, T value, TimeSpan? expiration = null)
        {
            _redisClient.StringSet(key, value, expiration ?? TimeSpan.FromDays(365));
        }

        public Task AddAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken token = default)
        {
            return _redisClient.StringSetAsync(key, value, expiration ?? TimeSpan.FromDays(365));
        }

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

        public T Get<T>(string key, Func<T> func, TimeSpan? expiration = null)
        {
            if (_redisClient.Exists(key)) return _redisClient.StringGet<T>(key);

            var value = func.Invoke();
            _redisClient.StringSet(key, value, expiration ?? TimeSpan.FromDays(365));

            return value;
        }

        public T Get<T>(string key)
        {
            return _redisClient.StringGet<T>(key);
        }

        public async Task<T> GetAsync<T>(string key, Func<Task<T>> func, TimeSpan? expiration = null,
            CancellationToken token = default)
        {
            if (await _redisClient.ExistsAsync(key)) return await _redisClient.StringGetAsync<T>(key);

            var value = await func.Invoke();
            await _redisClient.StringSetAsync(key, value, expiration ?? TimeSpan.FromDays(365));

            return value;
        }

        public Task<T> GetAsync<T>(string key, CancellationToken token = default)
        {
            return _redisClient.StringGetAsync<T>(key);
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

        public bool TryAdd<T>(string key, T value, TimeSpan? expiration = null)
        {
            if (_redisClient.Exists(key))
                return false;

            return _redisClient.StringSet(key, value, expiration ?? TimeSpan.FromDays(365));
        }

        public async Task<bool> TryAddAsync<T>(string key, T value, TimeSpan? expiration = null,
            CancellationToken token = default)
        {
            if (await _redisClient.ExistsAsync(key))
                return false;

            return await _redisClient.StringSetAsync(key, value, expiration ?? TimeSpan.FromDays(365));
        }
    }
}