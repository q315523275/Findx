using Findx.Extensions;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Redis.StackExchangeRedis
{
    public class StackExchangeRedisClient : IRedisClient
    {
        private readonly IConnectionPool _pool;
        private readonly RedisCacheOptions _options;
        private readonly IRedisSerializer _serializer;
        private readonly SemaphoreSlim _connectionLock;

        public StackExchangeRedisClient(IConnectionPool pool, RedisCacheOptions options, IRedisSerializer serializer)
        {
            _pool = pool;
            _options = options;
            _serializer = serializer;
            _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        }

        protected IDatabase DataBase { get; private set; }

        protected IDatabase DataBaseAsync { get; private set; }

        private void Connect()
        {
            if (DataBase != null)
                return;

            _connectionLock.Wait();

            try
            {
                if (DataBase != null)
                    return;

                DataBase = _pool.Acquire(_options.Configuration).GetDatabase();
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private async Task ConnectAsync()
        {
            if (DataBaseAsync != null)
                return;

            await _connectionLock.WaitAsync();

            try
            {
                if (DataBaseAsync != null)
                    return;

                DataBaseAsync = (await _pool.AcquireAsync(_options.Configuration)).GetDatabase();
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        #region Keys
        /// <summary>
        /// 设置Key前缀
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string SetPrefix(string key)
        {
            Check.NotNull(key, nameof(key));

            return _options.Prefix.IsNullOrWhiteSpace() ? key : $"{_options.Prefix}:{key}";
        }

        /// <summary>
        /// 查找当前命名前缀下共有多少个Key
        /// </summary>
        /// <returns></returns>
        public int KeyCount()
        {
            return CalcuteKeyCount("*");
        }

        /// <summary>
        /// 查找键名
        /// </summary>
        /// <param name="pattern">匹配项</param>
        /// <returns>匹配上的所有键名</returns>
        public IEnumerable<string> SearchKeys(string pattern)
        {
            var endpoints = _pool.Acquire(_options.Configuration)?.GetEndPoints();

            if (endpoints == null || !endpoints.Any())
                return null;

            return _pool.Acquire(_options.Configuration)
                        .GetServer(endpoints.First())
                        .Keys(pattern: pattern)
                        .Select(r => (string)r);
        }

        /// <summary>
        /// 查看缓存时间
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long TTL(string key)
        {
            Connect();

            var ts = DataBase.KeyTimeToLive(SetPrefix(key));
            return ts.HasValue ? (long)ts.Value.TotalSeconds : -1;
        }

        /// <summary>
        /// 查看缓存时间
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<long> TTLAsync(string key)
        {
            await ConnectAsync();

            var ts = await DataBaseAsync.KeyTimeToLiveAsync(SetPrefix(key));
            return ts.HasValue ? (long)ts.Value.TotalSeconds : -1;
        }

        /// <summary>
        /// 判断是否存在当前的Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            Connect();

            return DataBase.KeyExists(SetPrefix(key));
        }

        /// <summary>
        /// 判断是否存在当前的Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(string key)
        {
            await ConnectAsync();

            return await DataBaseAsync.KeyExistsAsync(SetPrefix(key));
        }

        /// <summary>
        /// 设置key的失效时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool Expire(string key, TimeSpan expiry)
        {
            Connect();

            return DataBase.KeyExpire(SetPrefix(key), expiry);
        }

        /// <summary>
        /// 设置key的失效时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public async Task<bool> ExpireAsync(string key, TimeSpan expiry)
        {
            await ConnectAsync();

            return await DataBaseAsync.KeyExpireAsync(SetPrefix(key), expiry);
        }

        /// <summary>
        /// 设置key的失效时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool Expire(string key, DateTime expiry)
        {
            Connect();

            return DataBase.KeyExpire(SetPrefix(key), expiry);
        }

        /// <summary>
        /// 设置key的失效时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public async Task<bool> ExpireAsync(string key, DateTime expiry)
        {
            await ConnectAsync();

            return await DataBaseAsync.KeyExpireAsync(SetPrefix(key), expiry);
        }

        /// <summary>
        /// 移除当前key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            Connect();

            return DataBase.KeyDelete(SetPrefix(key));
        }

        /// <summary>
        /// 移除当前key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> RemoveAsync(string key)
        {
            await ConnectAsync();

            return await DataBaseAsync.KeyDeleteAsync(SetPrefix(key));
        }

        /// <summary>
        /// 移除全部key
        /// </summary>
        /// <param name="keys"></param>
        public void RemoveAll(List<string> keys)
        {
            foreach (var key in keys)
            {
                Remove(key);
            }
        }

        /// <summary>
        /// 移除全部key
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public async Task RemoveAllAsync(List<string> keys)
        {
            foreach (var key in keys)
            {
                await RemoveAsync(key);
            }
        }

        #endregion Keys

        #region Public

        /// <summary>
        /// 清除key
        /// </summary>
        public void FlushDb()
        {
            Connect();

            var endPoints = DataBase.Multiplexer.GetEndPoints().ToList();

            endPoints.ForEach(endPoint =>
            {
                DataBase.Multiplexer.GetServer(endPoint).FlushDatabase(DataBase.Database);
            });
        }

        /// <summary>
        /// 清除key
        /// </summary>
        public async Task FlushDbAsync()
        {
            await ConnectAsync();

            var endPoints = DataBaseAsync.Multiplexer.GetEndPoints();

            foreach (var endpoint in endPoints)
            {
                await DataBaseAsync.Multiplexer.GetServer(endpoint).FlushDatabaseAsync(DataBaseAsync.Database);
            }
        }

        /// <summary>
        /// 清除当前db的所有数据
        /// </summary>
        public void Clear()
        {
            DeleteKeyWithKeyPrefix("*");
        }
        /// <summary>
        /// 清除当前db的所有数据
        /// </summary>
        public Task ClearAsync()
        {
            return DeleteKeyWithKeyPrefixAsync("*");
        }

        /// <summary>
        /// 计算当前prefix开头的key总数
        /// </summary>
        /// <param name="prefix">key前缀</param>
        /// <returns></returns>
        private int CalcuteKeyCount(string prefix)
        {
            Connect();

            var retVal = DataBase.ScriptEvaluate("return table.getn(redis.call('keys', ARGV[1]))", values: new RedisValue[] { SetPrefix(prefix) });
            if (retVal.IsNull)
            {
                return 0;
            }
            return (int)retVal;
        }

        /// <summary>
        /// 删除以当前prefix开头的所有key缓存
        /// </summary>
        /// <param name="prefix">key前缀</param>
        private void DeleteKeyWithKeyPrefix(string prefix)
        {
            Connect();

            DataBase.ScriptEvaluate(@"
                local keys = redis.call('keys', ARGV[1])
                for i=1,#keys,5000 do
                redis.call('del', unpack(keys, i, math.min(i+4999, #keys)))
                end", values: new RedisValue[] { SetPrefix(prefix) });
        }

        /// <summary>
        /// 删除以当前prefix开头的所有key缓存
        /// </summary>
        /// <param name="prefix">key前缀</param>
        private async Task DeleteKeyWithKeyPrefixAsync(string prefix)
        {
            await ConnectAsync();

            await DataBaseAsync.ScriptEvaluateAsync(@"
                local keys = redis.call('keys', ARGV[1])
                for i=1,#keys,5000 do
                redis.call('del', unpack(keys, i, math.min(i+4999, #keys)))
                end", values: new RedisValue[] { SetPrefix(prefix) });
        }
        #endregion Public

        #region StringSet

        /// <summary>
        /// 设置string键值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">值</param>
        /// <returns>成功返回true</returns>
        public bool StringSet<T>(string key, T value)
        {
            Connect();

            var objBytes = _serializer.Serialize(value);

            return DataBase.StringSet(SetPrefix(key), objBytes);
        }

        /// <summary>
        /// 设置string键值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">值</param>
        /// <returns>成功返回true</returns>
        public async Task<bool> StringSetAsync<T>(string key, T value)
        {
            await ConnectAsync();

            var objBytes = _serializer.Serialize(value);

            return await DataBaseAsync.StringSetAsync(SetPrefix(key), objBytes);
        }

        /// <summary>
        /// 设置string键值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">值</param>
        /// <param name="expiresIn">过期间隔</param>
        /// <returns>成功返回true</returns>
        public bool StringSet<T>(string key, T value, TimeSpan expiresIn)
        {
            Connect();

            var objBytes = _serializer.Serialize(value);
            return DataBase.StringSet(SetPrefix(key), objBytes, expiresIn);
        }

        /// <summary>
        /// 设置string键值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">值</param>
        /// <param name="expiresIn">过期间隔</param>
        /// <returns>成功返回true</returns>
        public async Task<bool> StringSetAsync<T>(string key, T value, TimeSpan expiresIn)
        {
            await ConnectAsync();

            var objBytes = _serializer.Serialize(value);
            return await DataBaseAsync.StringSetAsync(SetPrefix(key), objBytes, expiresIn);
        }

        /// <summary>
        /// 设置string键值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">值</param>
        /// <param name="expiresAt">过期时间</param>
        /// <returns>成功返回true</returns>
        public bool StringSet<T>(string key, T value, DateTimeOffset expiresAt)
        {
            Connect();

            var objBytes = _serializer.Serialize(value);

            var expiration = expiresAt.Subtract(DateTimeOffset.Now);

            return DataBase.StringSet(SetPrefix(key), objBytes, expiration);
        }

        /// <summary>
        /// 设置string键值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">值</param>
        /// <param name="expiresAt">过期时间</param>
        /// <returns>成功返回true</returns>
        public async Task<bool> StringSetAsync<T>(string key, T value, DateTimeOffset expiresAt)
        {
            await ConnectAsync();

            var objBytes = _serializer.Serialize(value);
            var expiration = expiresAt.Subtract(DateTimeOffset.Now);
            return await DataBaseAsync.StringSetAsync(SetPrefix(key), objBytes, expiration);
        }

        /// <summary>
        /// 批量设置string键值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="items">键值列表</param>
        /// <returns>成功返回true</returns>
        public bool StringSetAll<T>(IList<Tuple<string, T>> items)
        {
            Connect();

            var values = items.Select(m => new KeyValuePair<RedisKey, RedisValue>(SetPrefix(m.Item1), _serializer.Serialize(m.Item2))).ToArray();

            return DataBase.StringSet(values);
        }

        /// <summary>
        /// 批量设置string键值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="items">键值列表</param>
        /// <returns>成功返回true</returns>
        public async Task<bool> StringSetAllAsync<T>(IList<Tuple<string, T>> items)
        {
            await ConnectAsync();

            var values = items.Select(m => new KeyValuePair<RedisKey, RedisValue>(SetPrefix(m.Item1), _serializer.Serialize(m.Item2))).ToArray();

            return await DataBaseAsync.StringSetAsync(values);
        }

        /// <summary>
        /// string获取值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <returns></returns>
        public T StringGet<T>(string key)
        {
            Connect();

            var valuesBytes = DataBase.StringGet(SetPrefix(key));
            if (!valuesBytes.HasValue)
            {
                return default(T);
            }
            return _serializer.Deserialize<T>(valuesBytes);
        }

        /// <summary>
        /// string获取值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        public async Task<T> StringGetAsync<T>(string key)
        {
            await ConnectAsync();

            var valuesBytes = await DataBaseAsync.StringGetAsync(SetPrefix(key));
            if (!valuesBytes.HasValue)
            {
                return default;
            }
            return _serializer.Deserialize<T>(valuesBytes);
        }

        /// <summary>
        /// 键值累加
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">增长数量</param>
        /// <returns>累加后的值</returns>
        public long StringIncrement(string key, long value = 1)
        {
            Connect();

            return DataBase.StringIncrement(SetPrefix(key), value);
        }

        /// <summary>
        /// 键值累加
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">增长数量</param>
        /// <returns>累加后的值</returns>
        public async Task<long> StringIncrementAsync(string key, long value = 1)
        {
            await ConnectAsync();

            return await DataBaseAsync.StringIncrementAsync(SetPrefix(key), value);
        }

        /// <summary>
        /// 键值累加
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">增长数量</param>
        /// <returns>累加后的值</returns>
        public double StringIncrementDouble(string key, double value)
        {
            Connect();

            return DataBase.StringIncrement(SetPrefix(key), value);
        }

        /// <summary>
        /// 键值累加
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">增长数量</param>
        /// <returns>累加后的值</returns>
        public async Task<double> StringIncrementDoubleAsync(string key, double value)
        {
            await ConnectAsync();

            return await DataBaseAsync.StringIncrementAsync(SetPrefix(key), value);
        }

        /// <summary>
        /// 键值递减
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">减少数量</param>
        /// <returns>递减后的值</returns>
        public long StringDecrement(string key, long value = 1)
        {
            Connect();

            return DataBase.StringDecrement(SetPrefix(key), value);
        }

        /// <summary>
        /// 键值递减
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">减少数量</param>
        /// <returns>递减后的值</returns>
        public async Task<long> StringDecrementAsync(string key, long value = 1)
        {
            await ConnectAsync();

            return await DataBaseAsync.StringDecrementAsync(SetPrefix(key), value);
        }

        /// <summary>
        /// 键值递减
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">减少数量</param>
        /// <returns>递减后的值</returns>
        public double StringDecrementDouble(string key, double value)
        {
            Connect();

            return DataBase.StringDecrement(SetPrefix(key), value);
        }

        /// <summary>
        /// 键值递减
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">减少数量</param>
        /// <returns>递减后的值</returns>
        public async Task<double> StringDecrementDoubleAsync(string key, double value)
        {
            await ConnectAsync();

            return await DataBaseAsync.StringDecrementAsync(SetPrefix(key), value);
        }

        #endregion StringSet

        #region Hash

        /// <summary>
        /// 获取所有的Hash键
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="commandFlags"></param>
        /// <returns></returns>
        public IEnumerable<string> HashKeys(string key)
        {
            Connect();

            return DataBase.HashKeys(SetPrefix(key)).Select(x => x.ToString());
        }

        /// <summary>
        /// 获取hash键的个数
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="commandFlags"></param>
        /// <returns></returns>
        public long HashLength(string key)
        {
            Connect();

            return DataBase.HashLength(SetPrefix(key));
        }

        /// <summary>
        /// 设置一个hash值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="hashField">hash的键值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public bool HashSet<T>(string key, string hashField, T value)
        {
            Connect();

            return DataBase.HashSet(SetPrefix(key), hashField, _serializer.Serialize(value));
        }

        /// <summary>
        /// 设置一个hash值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="hashField">hash的键值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public async Task<bool> HashSetAsync<T>(string key, string hashField, T value)
        {
            await ConnectAsync();

            var objBytes = _serializer.Serialize(value);
            return await DataBaseAsync.HashSetAsync(SetPrefix(key), hashField, objBytes);
        }

        /// <summary>
        /// 批量设置hash值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="values">键值对</param>
        public void HashSet<T>(string key, Dictionary<string, T> values)
        {
            Connect();

            var entries = values.Select(kv => new HashEntry(kv.Key, _serializer.Serialize(kv.Value)));

            DataBase.HashSet(SetPrefix(key), entries.ToArray());
        }

        /// <summary>
        /// 批量设置hash值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="values">键值对</param>
        public async Task HashSetAsync<T>(string key, Dictionary<string, T> values)
        {
            await ConnectAsync();

            var entries = values.Select(kv => new HashEntry(kv.Key, _serializer.Serialize(kv.Value)));
            await DataBaseAsync.HashSetAsync(SetPrefix(key), entries.ToArray());
        }

        /// <summary>
        /// 获取一个hash值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <returns></returns>
        public T HashGet<T>(string key, string hashField)
        {
            Connect();

            var redisValue = DataBase.HashGet(SetPrefix(key), hashField);

            return redisValue.HasValue ? _serializer.Deserialize<T>(redisValue) : default(T);
        }

        /// <summary>
        /// 获取一个hash值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <returns></returns>
        public async Task<T> HashGetAsync<T>(string key, string hashField)
        {
            await ConnectAsync();

            var redisValue = await DataBaseAsync.HashGetAsync(SetPrefix(key), hashField);

            return redisValue.HasValue ? _serializer.Deserialize<T>(redisValue) : default(T);
        }

        /// <summary>
        /// 获取hash值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="hashFields">hash键组合</param>
        /// <returns></returns>
        public Dictionary<string, T> HashGet<T>(string key, IEnumerable<string> hashFields)
        {
            var result = new Dictionary<string, T>();
            foreach (var hashField in hashFields)
            {
                var value = HashGet<T>(key, hashField);
                result.Add(key, value);
            }
            return result;
        }

        /// <summary>
        /// 获取hash值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="hashFields">hash键组合</param>
        /// <returns></returns>
        public async Task<Dictionary<string, T>> HashGetAsync<T>(string key, IEnumerable<string> hashFields)
        {
            var result = new Dictionary<string, T>();
            foreach (var hashField in hashFields)
            {
                var value = await HashGetAsync<T>(key, hashField);
                result.Add(key, value);
            }
            return result;
        }

        /// <summary>
        /// 获取全部hash值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">key</param>
        /// <returns></returns>
        public Dictionary<string, T> HashGetAll<T>(string key)
        {
            Connect();

            return (DataBase.HashGetAll(SetPrefix(key)))
                             .ToDictionary(x => x.Name.ToString(), x => _serializer.Deserialize<T>(x.Value), StringComparer.Ordinal);
        }

        /// <summary>
        /// 获取全部hash值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">key</param>
        /// <returns></returns>
        public async Task<Dictionary<string, T>> HashGetAllAsync<T>(string key)
        {
            await ConnectAsync();

            return (await DataBaseAsync.HashGetAllAsync(SetPrefix(key)))
                                        .ToDictionary(x => x.Name.ToString(), x => _serializer.Deserialize<T>(x.Value), StringComparer.Ordinal);
        }

        /// <summary>
        /// 获取全部hash值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">key</param>
        /// <returns></returns>
        public IEnumerable<T> HashValues<T>(string key)
        {
            Connect();

            return DataBase.HashValues(SetPrefix(key)).Select(m => _serializer.Deserialize<T>(m));
        }

        /// <summary>
        /// 获取全部hash值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">key</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> HashValuesAsync<T>(string key)
        {
            await ConnectAsync();

            return (await DataBaseAsync.HashValuesAsync(SetPrefix(key))).Select(m => _serializer.Deserialize<T>(m));
        }

        /// <summary>
        /// 判断是否存在hash键
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <returns></returns>
        public bool HashExists(string key, string hashField)
        {
            Connect();

            return DataBase.HashExists(SetPrefix(key), hashField);
        }

        /// <summary>
        /// 判断是否存在hash键
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <returns></returns>
        public async Task<bool> HashExistsAsync(string key, string hashField)
        {
            await ConnectAsync();

            return await DataBaseAsync.HashExistsAsync(SetPrefix(key), hashField);
        }

        /// <summary>
        /// 删除一个hash键
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <returns></returns>
        public bool HashDelete(string key, string hashField)
        {
            Connect();

            return DataBase.HashDelete(SetPrefix(key), hashField);
        }

        /// <summary>
        /// 删除一个hash键
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <returns></returns>
        public async Task<bool> HashDeleteAsync(string key, string hashField)
        {
            await ConnectAsync();

            return await DataBaseAsync.HashDeleteAsync(SetPrefix(key), hashField);
        }

        /// <summary>
        /// 删除hash键
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashFields">hash键集合</param>
        /// <returns></returns>
        public long HashDelete(string key, IEnumerable<string> hashFields)
        {
            Connect();

            return DataBase.HashDelete(SetPrefix(key), hashFields.Select(x => (RedisValue)x).ToArray());
        }

        /// <summary>
        /// 删除hash键
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashFields">hash键集合</param>
        /// <returns></returns>
        public async Task<long> HashDeleteAsync(string key, IEnumerable<string> hashFields)
        {
            await ConnectAsync();

            return await DataBaseAsync.HashDeleteAsync(SetPrefix(key), hashFields.Select(x => (RedisValue)x).ToArray());
        }

        /// <summary>
        /// hash递增
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <param name="value">递增值</param>
        /// <returns></returns>
        public long HashIncrement(string key, string hashField, long value = 1)
        {
            Connect();

            return DataBase.HashIncrement(SetPrefix(key), hashField, value);
        }

        /// <summary>
        /// hash递增
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <param name="value">递增值</param>
        /// <returns></returns>
        public async Task<long> HashIncrementAsync(string key, string hashField, long value = 1)
        {
            await ConnectAsync();

            return await DataBaseAsync.HashIncrementAsync(SetPrefix(key), hashField, value);
        }

        /// <summary>
        /// hash递减
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <param name="value">递减值</param>
        /// <returns></returns>
        public long HashDecrement(string key, string hashField, long value = 1)
        {
            Connect();

            return DataBase.HashDecrement(SetPrefix(key), hashField, value);
        }

        /// <summary>
        /// hash递减
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <param name="value">递减值</param>
        /// <returns></returns>
        public async Task<long> HashDecrementAsync(string key, string hashField, long value = 1)
        {
            await ConnectAsync();

            return await DataBaseAsync.HashDecrementAsync(SetPrefix(key), hashField, value);
        }

        /// <summary>
        /// hash递增
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <param name="value">递增值</param>
        /// <returns></returns>
        public double HashIncrementDouble(string key, string hashField, double value)
        {
            Connect();

            return DataBase.HashIncrement(SetPrefix(key), hashField, value);
        }

        /// <summary>
        /// hash递增
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <param name="value">递增值</param>
        /// <returns></returns>
        public async Task<double> HashIncrementDoubleAsync(string key, string hashField, double value)
        {
            await ConnectAsync();

            return await DataBaseAsync.HashIncrementAsync(SetPrefix(key), hashField, value);
        }

        /// <summary>
        /// hash递减
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <param name="value">递减值</param>
        /// <returns></returns>
        public double HashDecrementDouble(string key, string hashField, double value)
        {
            Connect();

            return DataBase.HashDecrement(SetPrefix(key), hashField, value);
        }

        /// <summary>
        /// hash递减
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <param name="value">递减值</param>
        /// <returns></returns>
        public async Task<double> HashDecrementDoubleAsync(string key, string hashField, double value)
        {
            await ConnectAsync();

            return await DataBaseAsync.HashDecrementAsync(SetPrefix(key), hashField, value);
        }

        #endregion hash

        #region Lock

        /// <summary>
        /// 获取一个锁
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">键名</param>
        /// <param name="value">值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>成功返回true</returns>
        public bool LockTake<T>(string key, T value, TimeSpan expiry)
        {
            Connect();

            var objBytes = _serializer.Serialize(value);
            return DataBase.LockTake(SetPrefix(key), objBytes, expiry);
        }

        /// <summary>
        /// 异步获取一个锁
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">键名</param>
        /// <param name="value">值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>成功返回true</returns>
        public async Task<bool> LockTakeAsync<T>(string key, T value, TimeSpan expiry)
        {
            await ConnectAsync();

            var objBytes = _serializer.Serialize(value);
            return await DataBaseAsync.LockTakeAsync(SetPrefix(key), objBytes, expiry);
        }

        /// <summary>
        /// 释放一个锁
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">键名</param>
        /// <param name="value">值</param>
        /// <returns>成功返回true</returns>
        public bool LockRelease<T>(string key, T value)
        {
            Connect();

            var objBytes = _serializer.Serialize(value);
            return DataBase.LockRelease(SetPrefix(key), objBytes);
        }

        /// <summary>
        /// 异步释放一个锁
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">键名</param>
        /// <param name="value">值</param>
        /// <returns>成功返回true</returns>
        public async Task<bool> LockReleaseAsync<T>(string key, T value)
        {
            await ConnectAsync();

            var objBytes = _serializer.Serialize(value);
            return await DataBaseAsync.LockReleaseAsync(SetPrefix(key), objBytes);
        }

        #endregion lock
    }
}
