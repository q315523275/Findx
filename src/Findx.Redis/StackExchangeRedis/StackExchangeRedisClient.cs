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
        private readonly IStackExchangeRedisDataBaseProvider _dataBaseProvider;
        private readonly RedisOptions _options;
        private readonly IRedisSerializer _serializer;
        private readonly SemaphoreSlim _connectionLock;

        public StackExchangeRedisClient(IStackExchangeRedisDataBaseProvider dataBaseProvider, RedisOptions options, IRedisSerializer serializer)
        {
            _dataBaseProvider = dataBaseProvider;
            _options = options;
            _serializer = serializer;
            _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
            Name = options.Name;
        }

        #region 私有

        private IDatabase _cache;

        private IDatabase _cacheAsync;

        private void Connect()
        {
            if (_cache != null)
                return;

            _connectionLock.Wait();

            try
            {
                if (_cache != null)
                    return;

                _cache = _dataBaseProvider.GetConnection(_options).GetDatabase();
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (_cacheAsync != null)
                return;

            await _connectionLock.WaitAsync();

            try
            {
                if (_cacheAsync != null)
                    return;

                _cacheAsync = (await _dataBaseProvider.GetConnectionAsync(_options, cancellationToken)).GetDatabase();
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        #endregion

        #region Public

        public string Name { get; }

        /// <summary>
        /// 清除key
        /// </summary>
        public void FlushDb()
        {
            Connect();

            var endPoints = _cache.Multiplexer.GetEndPoints().ToList();

            endPoints.ForEach(endPoint =>
            {
                _cache.Multiplexer.GetServer(endPoint).FlushDatabase(_cache.Database);
            });
        }

        /// <summary>
        /// 清除key
        /// </summary>
        public async Task FlushDbAsync()
        {
            await ConnectAsync();

            var endPoints = _cacheAsync.Multiplexer.GetEndPoints();

            foreach (var endpoint in endPoints)
            {
                await _cacheAsync.Multiplexer.GetServer(endpoint).FlushDatabaseAsync(_cacheAsync.Database);
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
        /// 执行redis命令
        /// </summary>
        /// <param name="script"></param>
        /// <param name="cacheKey"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object Eval(string script, string cacheKey, List<object> args)
        {
            Check.NotNullOrWhiteSpace(script, nameof(script));
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var redisKey = new RedisKey[] { cacheKey };

            var redisValues = new List<RedisValue>();

            foreach (var item in args)
            {
                if (item.GetType().Equals(typeof(byte[])))
                {
                    redisValues.Add((byte[])item);
                }
                else
                {
                    redisValues.Add(item.ToString());
                }
            }

            var res = _cache.ScriptEvaluate(script, redisKey, redisValues.ToArray());

            return res;
        }

        /// <summary>
        /// 执行redis命令
        /// </summary>
        /// <param name="script"></param>
        /// <param name="cacheKey"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<object> EvalAsync(string script, string cacheKey, List<object> args)
        {
            Check.NotNullOrWhiteSpace(script, nameof(script));
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var redisKey = new RedisKey[] { cacheKey };

            var redisValues = new List<RedisValue>();

            foreach (var item in args)
            {
                if (item.GetType().Equals(typeof(byte[])))
                {
                    redisValues.Add((byte[])item);
                }
                else
                {
                    redisValues.Add(item.ToString());
                }
            }

            var res = await _cacheAsync.ScriptEvaluateAsync(script, redisKey, redisValues.ToArray());

            return res;
        }

        #endregion Public

        #region Keys

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
            var endpoints = _dataBaseProvider.GetConnection(_options)?.GetEndPoints();

            if (endpoints == null || !endpoints.Any())
                return null;

            return _dataBaseProvider.GetConnection(_options)
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

            var ts = _cache.KeyTimeToLive(key);
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

            var ts = await _cacheAsync.KeyTimeToLiveAsync(key);
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

            return _cache.KeyExists(key);
        }

        /// <summary>
        /// 判断是否存在当前的Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(string key)
        {
            await ConnectAsync();

            return await _cacheAsync.KeyExistsAsync(key);
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

            return _cache.KeyExpire(key, expiry);
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

            return await _cacheAsync.KeyExpireAsync(key, expiry);
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

            return _cache.KeyExpire(key, expiry);
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

            return await _cacheAsync.KeyExpireAsync(key, expiry);
        }

        /// <summary>
        /// 移除当前key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            Connect();

            return _cache.KeyDelete(key);
        }

        /// <summary>
        /// 移除当前key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> RemoveAsync(string key)
        {
            await ConnectAsync();

            return await _cacheAsync.KeyDeleteAsync(key);
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

        /// <summary>
        /// 计算当前prefix开头的key总数
        /// </summary>
        /// <param name="prefix">key前缀</param>
        /// <returns></returns>
        public int CalcuteKeyCount(string prefix)
        {
            Connect();

            var retVal = _cache.ScriptEvaluate("return table.getn(redis.call('keys', ARGV[1]))", values: new RedisValue[] { prefix });
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
        public void DeleteKeyWithKeyPrefix(string prefix)
        {
            Connect();

            _cache.ScriptEvaluate(@"
                local keys = redis.call('keys', ARGV[1])
                for i=1,#keys,5000 do
                redis.call('del', unpack(keys, i, math.min(i+4999, #keys)))
                end", values: new RedisValue[] { prefix });
        }

        /// <summary>
        /// 删除以当前prefix开头的所有key缓存
        /// </summary>
        /// <param name="prefix">key前缀</param>
        public async Task DeleteKeyWithKeyPrefixAsync(string prefix)
        {
            await ConnectAsync();

            await _cacheAsync.ScriptEvaluateAsync(@"
                local keys = redis.call('keys', ARGV[1])
                for i=1,#keys,5000 do
                redis.call('del', unpack(keys, i, math.min(i+4999, #keys)))
                end", values: new RedisValue[] { prefix });
        }

        #endregion Keys

        #region StringSet(字符操作)

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

            return _cache.StringSet(key, objBytes, when: When.Always);
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

            return await _cacheAsync.StringSetAsync(key, objBytes);
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
            return _cache.StringSet(key, objBytes, expiresIn);
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
            return await _cacheAsync.StringSetAsync(key, objBytes, expiresIn);
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

            return _cache.StringSet(key, objBytes, expiration);
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
            return await _cacheAsync.StringSetAsync(key, objBytes, expiration);
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

            var values = items.Select(m => new KeyValuePair<RedisKey, RedisValue>(m.Item1, _serializer.Serialize(m.Item2))).ToArray();

            return _cache.StringSet(values);
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

            var values = items.Select(m => new KeyValuePair<RedisKey, RedisValue>(m.Item1, _serializer.Serialize(m.Item2))).ToArray();

            return await _cacheAsync.StringSetAsync(values);
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

            var valuesBytes = _cache.StringGet(key);
            if (!valuesBytes.HasValue)
            {
                return default;
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

            var valuesBytes = await _cacheAsync.StringGetAsync(key);
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

            return _cache.StringIncrement(key, value);
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

            return await _cacheAsync.StringIncrementAsync(key, value);
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

            return _cache.StringIncrement(key, value);
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

            return await _cacheAsync.StringIncrementAsync(key, value);
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

            return _cache.StringDecrement(key, value);
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

            return await _cacheAsync.StringDecrementAsync(key, value);
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

            return _cache.StringDecrement(key, value);
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

            return await _cacheAsync.StringDecrementAsync(key, value);
        }

        #endregion StringSet

        #region Hash(哈希操作)

        /// <summary>
        /// 获取所有的Hash键
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="commandFlags"></param>
        /// <returns></returns>
        public IEnumerable<string> HashKeys(string key)
        {
            Connect();

            return _cache.HashKeys(key).Select(x => x.ToString());
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

            return _cache.HashLength(key);
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

            return _cache.HashSet(key, hashField, _serializer.Serialize(value));
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
            return await _cacheAsync.HashSetAsync(key, hashField, objBytes);
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

            _cache.HashSet(key, entries.ToArray());
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
            await _cacheAsync.HashSetAsync(key, entries.ToArray());
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

            var redisValue = _cache.HashGet(key, hashField);

            return redisValue.HasValue ? _serializer.Deserialize<T>(redisValue) : default;
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

            var redisValue = await _cacheAsync.HashGetAsync(key, hashField);

            return redisValue.HasValue ? _serializer.Deserialize<T>(redisValue) : default;
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

            return (_cache.HashGetAll(key))
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

            return (await _cacheAsync.HashGetAllAsync(key))
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

            return _cache.HashValues(key).Select(m => _serializer.Deserialize<T>(m));
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

            return (await _cacheAsync.HashValuesAsync(key)).Select(m => _serializer.Deserialize<T>(m));
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

            return _cache.HashExists(key, hashField);
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

            return await _cacheAsync.HashExistsAsync(key, hashField);
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

            return _cache.HashDelete(key, hashField);
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

            return await _cacheAsync.HashDeleteAsync(key, hashField);
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

            return _cache.HashDelete(key, hashFields.Select(x => (RedisValue)x).ToArray());
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

            return await _cacheAsync.HashDeleteAsync(key, hashFields.Select(x => (RedisValue)x).ToArray());
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

            return _cache.HashIncrement(key, hashField, value);
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

            return await _cacheAsync.HashIncrementAsync(key, hashField, value);
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

            return _cache.HashDecrement(key, hashField, value);
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

            return await _cacheAsync.HashDecrementAsync(key, hashField, value);
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

            return _cache.HashIncrement(key, hashField, value);
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

            return await _cacheAsync.HashIncrementAsync(key, hashField, value);
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

            return _cache.HashDecrement(key, hashField, value);
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

            return await _cacheAsync.HashDecrementAsync(key, hashField, value);
        }

        #endregion hash

        #region Lock(锁操作)

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
            return _cache.LockTake(key, objBytes, expiry);
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
            return await _cacheAsync.LockTakeAsync(key, objBytes, expiry);
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
            return _cache.LockRelease(key, objBytes);
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
            return await _cacheAsync.LockReleaseAsync(key, objBytes);
        }

        #endregion lock

        #region List(集合操作)
        public T ListGetByIndex<T>(string cacheKey, long index)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var bytes = _cache.ListGetByIndex(cacheKey, index);
            return _serializer.Deserialize<T>(bytes);
        }

        public long ListLength(string cacheKey)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            return _cache.ListLength(cacheKey);
        }

        public T ListLeftPop<T>(string cacheKey)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var bytes = _cache.ListLeftPop(cacheKey);
            return _serializer.Deserialize<T>(bytes);
        }

        public long ListLeftPush<T>(string cacheKey, IList<T> cacheValues)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNull(cacheValues, nameof(cacheValues));

            Connect();

            var list = new List<RedisValue>();

            foreach (var item in cacheValues)
            {
                list.Add(_serializer.Serialize(item));
            }

            var len = _cache.ListLeftPush(cacheKey, list.ToArray());
            return len;
        }

        public List<T> ListRange<T>(string cacheKey, long start, long stop)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var list = new List<T>();

            var bytes = _cache.ListRange(cacheKey, start, stop);

            foreach (var item in bytes)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }

            return list;
        }

        public long ListRemove<T>(string cacheKey, long count, T cacheValue)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var bytes = _serializer.Serialize(cacheValue);
            return _cache.ListRemove(cacheKey, bytes, count);
        }

        public bool ListSetByIndex<T>(string cacheKey, long index, T cacheValue)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var bytes = _serializer.Serialize(cacheValue);
            _cache.ListSetByIndex(cacheKey, index, bytes);
            return true;
        }

        public bool ListTrim(string cacheKey, long start, long stop)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            _cache.ListTrim(cacheKey, start, stop);
            return true;
        }

        public long ListLeftPush<T>(string cacheKey, T cacheValue)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var bytes = _serializer.Serialize(cacheValue);
            return _cache.ListLeftPush(cacheKey, bytes);
        }

        public long ListInsertBefore<T>(string cacheKey, T pivot, T cacheValue)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var pivotBytes = _serializer.Serialize(pivot);
            var cacheValueBytes = _serializer.Serialize(cacheValue);
            return _cache.ListInsertBefore(cacheKey, pivotBytes, cacheValueBytes);
        }

        public long ListInsertAfter<T>(string cacheKey, T pivot, T cacheValue)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var pivotBytes = _serializer.Serialize(pivot);
            var cacheValueBytes = _serializer.Serialize(cacheValue);
            return _cache.ListInsertAfter(cacheKey, pivotBytes, cacheValueBytes);
        }

        public long ListRightPush<T>(string cacheKey, T cacheValue)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var bytes = _serializer.Serialize(cacheValue);
            return _cache.ListRightPush(cacheKey, bytes);
        }

        public long ListRightPush<T>(string cacheKey, IList<T> cacheValues)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNull(cacheValues, nameof(cacheValues));

            Connect();

            var list = new List<RedisValue>();

            foreach (var item in cacheValues)
            {
                list.Add(_serializer.Serialize(item));
            }

            var len = _cache.ListRightPush(cacheKey, list.ToArray());
            return len;
        }

        public T ListRightPop<T>(string cacheKey)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var bytes = _cache.ListRightPop(cacheKey);
            return _serializer.Deserialize<T>(bytes);
        }

        public async Task<T> ListGetByIndexAsync<T>(string cacheKey, long index)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var bytes = await _cacheAsync.ListGetByIndexAsync(cacheKey, index);
            return _serializer.Deserialize<T>(bytes);
        }

        public async Task<long> ListLengthAsync(string cacheKey)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            return await _cacheAsync.ListLengthAsync(cacheKey);
        }

        public async Task<T> ListLeftPopAsync<T>(string cacheKey)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var bytes = await _cacheAsync.ListLeftPopAsync(cacheKey);
            return _serializer.Deserialize<T>(bytes);
        }

        public async Task<long> ListLeftPushAsync<T>(string cacheKey, T cacheValue)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var bytes = _serializer.Serialize(cacheValue);
            return await _cacheAsync.ListLeftPushAsync(cacheKey, bytes);
        }

        public async Task<long> ListLeftPushAsync<T>(string cacheKey, IList<T> cacheValues)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNull(cacheValues, nameof(cacheValues));

            await ConnectAsync();

            var list = new List<RedisValue>();

            foreach (var item in cacheValues)
            {
                list.Add(_serializer.Serialize(item));
            }

            var len = await _cacheAsync.ListLeftPushAsync(cacheKey, list.ToArray());
            return len;
        }

        public async Task<T> ListRightPopAsync<T>(string cacheKey)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var bytes = await _cacheAsync.ListRightPopAsync(cacheKey);
            return _serializer.Deserialize<T>(bytes);
        }

        public async Task<long> ListRightPushAsync<T>(string cacheKey, T cacheValue)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var bytes = _serializer.Serialize(cacheValue);
            return await _cacheAsync.ListRightPushAsync(cacheKey, bytes);
        }

        public async Task<long> ListRightPushAsync<T>(string cacheKey, IList<T> cacheValues)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNull(cacheValues, nameof(cacheValues));

            await ConnectAsync();

            var list = new List<RedisValue>();

            foreach (var item in cacheValues)
            {
                list.Add(_serializer.Serialize(item));
            }

            var len = await _cacheAsync.ListRightPushAsync(cacheKey, list.ToArray());
            return len;
        }

        public async Task<List<T>> ListRangeAsync<T>(string cacheKey, long start, long stop)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var list = new List<T>();

            var bytes = await _cacheAsync.ListRangeAsync(cacheKey, start, stop);

            foreach (var item in bytes)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }

            return list;
        }

        public async Task<long> ListRemoveAsync<T>(string cacheKey, long count, T cacheValue)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var bytes = _serializer.Serialize(cacheValue);
            return await _cacheAsync.ListRemoveAsync(cacheKey, bytes, count);
        }

        public async Task<bool> ListSetByIndexAsync<T>(string cacheKey, long index, T cacheValue)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var bytes = _serializer.Serialize(cacheValue);
            await _cacheAsync.ListSetByIndexAsync(cacheKey, index, bytes);
            return true;
        }

        public async Task<bool> ListTrimAsync(string cacheKey, long start, long stop)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            await _cacheAsync.ListTrimAsync(cacheKey, start, stop);
            return true;
        }

        public async Task<long> ListInsertBeforeAsync<T>(string cacheKey, T pivot, T cacheValue)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var pivotBytes = _serializer.Serialize(pivot);
            var cacheValueBytes = _serializer.Serialize(cacheValue);
            return await _cacheAsync.ListInsertBeforeAsync(cacheKey, pivotBytes, cacheValueBytes);
        }

        public async Task<long> ListInsertAfterAsync<T>(string cacheKey, T pivot, T cacheValue)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var pivotBytes = _serializer.Serialize(pivot);
            var cacheValueBytes = _serializer.Serialize(cacheValue);
            return await _cacheAsync.ListInsertAfterAsync(cacheKey, pivotBytes, cacheValueBytes);
        }

        #endregion

        #region Set(数组操作)
        public long SetAdd<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNull(cacheValues, nameof(cacheValues));

            Connect();

            var list = new List<RedisValue>();

            foreach (var item in cacheValues)
            {
                list.Add(_serializer.Serialize(item));
            }

            var len = _cache.SetAdd(cacheKey, list.ToArray());

            if (expiration.HasValue)
            {
                _cache.KeyExpire(cacheKey, expiration.Value);
            }

            return len;
        }

        public long SetLength(string cacheKey)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var len = _cache.SetLength(cacheKey);
            return len;
        }

        public bool SetContains<T>(string cacheKey, T cacheValue)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var bytes = _serializer.Serialize(cacheValue);

            var flag = _cache.SetContains(cacheKey, bytes);
            return flag;
        }

        public List<T> SetMembers<T>(string cacheKey)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var list = new List<T>();

            var bytes = _cache.SetMembers(cacheKey);

            foreach (var item in bytes)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }

            return list;
        }

        public T SetPop<T>(string cacheKey)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var bytes = _cache.SetPop(cacheKey);

            return _serializer.Deserialize<T>(bytes);
        }

        public List<T> SetRandomMembers<T>(string cacheKey, int count = 1)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var list = new List<T>();

            var bytes = _cache.SetRandomMembers(cacheKey, count);

            foreach (var item in bytes)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }

            return list;
        }

        public long SetRemove<T>(string cacheKey, IList<T> cacheValues = null)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var len = 0L;

            if (cacheValues != null && cacheValues.Any())
            {
                var bytes = new List<RedisValue>();

                foreach (var item in cacheValues)
                {
                    bytes.Add(_serializer.Serialize<T>(item));
                }

                len = _cache.SetRemove(cacheKey, bytes.ToArray());
            }
            else
            {
                var flag = _cache.KeyDelete(cacheKey);
                len = flag ? 1 : 0;
            }

            return len;
        }

        public async Task<long> SetAddAsync<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNull(cacheValues, nameof(cacheValues));

            await ConnectAsync();

            var list = new List<RedisValue>();

            foreach (var item in cacheValues)
            {
                list.Add(_serializer.Serialize(item));
            }

            var len = await _cacheAsync.SetAddAsync(cacheKey, list.ToArray());

            if (expiration.HasValue)
            {
                await _cacheAsync.KeyExpireAsync(cacheKey, expiration.Value);
            }

            return len;
        }

        public async Task<long> SetLengthAsync(string cacheKey)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var len = await _cacheAsync.SetLengthAsync(cacheKey);
            return len;
        }

        public async Task<bool> SetContainsAsync<T>(string cacheKey, T cacheValue)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var bytes = _serializer.Serialize(cacheValue);

            var flag = await _cacheAsync.SetContainsAsync(cacheKey, bytes);
            return flag;
        }

        public async Task<List<T>> SetMembersAsync<T>(string cacheKey)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var list = new List<T>();

            var vals = await _cacheAsync.SetMembersAsync(cacheKey);

            foreach (var item in vals)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }

            return list;
        }

        public async Task<T> SetPopAsync<T>(string cacheKey)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var bytes = await _cacheAsync.SetPopAsync(cacheKey);

            return _serializer.Deserialize<T>(bytes);
        }

        public async Task<List<T>> SetRandomMembersAsync<T>(string cacheKey, int count = 1)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var list = new List<T>();

            var bytes = await _cacheAsync.SetRandomMembersAsync(cacheKey, count);

            foreach (var item in bytes)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }

            return list;
        }

        public async Task<long> SetRemoveAsync<T>(string cacheKey, IList<T> cacheValues = null)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var len = 0L;

            if (cacheValues != null && cacheValues.Any())
            {
                var bytes = new List<RedisValue>();

                foreach (var item in cacheValues)
                {
                    bytes.Add(_serializer.Serialize<T>(item));
                }

                len = await _cacheAsync.SetRemoveAsync(cacheKey, bytes.ToArray());
            }
            else
            {
                var flag = await _cacheAsync.KeyDeleteAsync(cacheKey);
                len = flag ? 1 : 0;
            }

            return len;
        }
        #endregion

        #region Sorted Set(有序数组)
        public long SortedSetAdd<T>(string cacheKey, Dictionary<T, double> cacheValues)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var param = new List<SortedSetEntry>();

            foreach (var item in cacheValues)
            {
                param.Add(new SortedSetEntry(_serializer.Serialize(item.Key), item.Value));
            }

            var len = _cache.SortedSetAdd(cacheKey, param.ToArray());

            return len;
        }

        public long SortedSetLength(string cacheKey)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var len = _cache.SortedSetLength(cacheKey);
            return len;
        }

        public long SortedSetLengthByValue(string cacheKey, double min, double max)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var len = _cache.SortedSetLengthByValue(cacheKey, min, max);
            return len;
        }

        public double SortedSetIncrement(string cacheKey, string field, double val = 1)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNullOrWhiteSpace(field, nameof(field));

            Connect();

            var value = _cache.SortedSetIncrement(cacheKey, field, val);
            return value;
        }

        public long SortedSetLengthByValue(string cacheKey, string min, string max)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var len = _cache.SortedSetLengthByValue(cacheKey, min, max);
            return len;
        }

        public List<T> SortedSetRangeByRank<T>(string cacheKey, long start, long stop)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var list = new List<T>();

            var bytes = _cache.SortedSetRangeByRank(cacheKey, start, stop);

            foreach (var item in bytes)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }

            return list;
        }

        public long? SortedSetRank<T>(string cacheKey, T cacheValue)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var bytes = _serializer.Serialize(cacheValue);

            var index = _cache.SortedSetRank(cacheKey, bytes);

            return index;
        }

        public long SortedSetRemove<T>(string cacheKey, IList<T> cacheValues)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var bytes = new List<RedisValue>();

            foreach (var item in cacheValues)
            {
                bytes.Add(_serializer.Serialize(item));
            }

            var len = _cache.SortedSetRemove(cacheKey, bytes.ToArray());

            return len;
        }

        public double? SortedSetScore<T>(string cacheKey, T cacheValue)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var bytes = _serializer.Serialize(cacheValue);

            var score = _cache.SortedSetScore(cacheKey, bytes);

            return score;
        }

        public async Task<long> SortedSetAddAsync<T>(string cacheKey, Dictionary<T, double> cacheValues)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var param = new List<SortedSetEntry>();

            foreach (var item in cacheValues)
            {
                param.Add(new SortedSetEntry(_serializer.Serialize(item.Key), item.Value));
            }

            var len = await _cacheAsync.SortedSetAddAsync(cacheKey, param.ToArray());

            return len;
        }

        public async Task<long> SortedSetLengthAsync(string cacheKey)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var len = await _cacheAsync.SortedSetLengthAsync(cacheKey);
            return len;
        }

        public async Task<long> SortedSetLengthByValueAsync(string cacheKey, double min, double max)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var len = await _cacheAsync.SortedSetLengthByValueAsync(cacheKey, min, max);
            return len;
        }

        public async Task<double> SortedSetIncrementAsync(string cacheKey, string field, double val = 1)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNullOrWhiteSpace(field, nameof(field));

            await ConnectAsync();

            var value = await _cacheAsync.SortedSetIncrementAsync(cacheKey, field, val);
            return value;
        }

        public async Task<long> SortedSetLengthByValueAsync(string cacheKey, string min, string max)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var len = await _cacheAsync.SortedSetLengthByValueAsync(cacheKey, min, max);
            return len;
        }

        public async Task<List<T>> SortedSetRangeByRankAsync<T>(string cacheKey, long start, long stop)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var list = new List<T>();

            var bytes = await _cacheAsync.SortedSetRangeByRankAsync(cacheKey, start, stop);

            foreach (var item in bytes)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }

            return list;
        }

        public async Task<long?> SortedSetRankAsync<T>(string cacheKey, T cacheValue)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var bytes = _serializer.Serialize(cacheValue);

            var index = await _cacheAsync.SortedSetRankAsync(cacheKey, bytes);

            return index;
        }

        public async Task<long> SortedSetRemoveAsync<T>(string cacheKey, IList<T> cacheValues)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var bytes = new List<RedisValue>();

            foreach (var item in cacheValues)
            {
                bytes.Add(_serializer.Serialize(item));
            }

            var len = await _cacheAsync.SortedSetRemoveAsync(cacheKey, bytes.ToArray());

            return len;
        }

        public async Task<double?> SortedSetScoreAsync<T>(string cacheKey, T cacheValue)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await ConnectAsync();

            var bytes = _serializer.Serialize(cacheValue);

            var score = await _cacheAsync.SortedSetScoreAsync(cacheKey, bytes);

            return score;
        }
        #endregion

        #region Geo(经纬度操作)
        public long GeoAdd(string cacheKey, List<(double longitude, double latitude, string member)> values)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNull(values, nameof(values));

            Connect();

            var list = new List<GeoEntry>();

            foreach (var item in values)
            {
                list.Add(new GeoEntry(item.longitude, item.latitude, item.member));
            }

            var res = _cache.GeoAdd(cacheKey, list.ToArray());
            return res;
        }

        public async Task<long> GeoAddAsync(string cacheKey, List<(double longitude, double latitude, string member)> values)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNull(values, nameof(values));

            await ConnectAsync();

            var list = new List<GeoEntry>();

            foreach (var item in values)
            {
                list.Add(new GeoEntry(item.longitude, item.latitude, item.member));
            }

            var res = await _cacheAsync.GeoAddAsync(cacheKey, list.ToArray());
            return res;
        }

        public double? GeoDistance(string cacheKey, string member1, string member2, string unit = "m")
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNullOrWhiteSpace(member1, nameof(member1));
            Check.NotNullOrWhiteSpace(member2, nameof(member2));
            Check.NotNullOrWhiteSpace(unit, nameof(unit));

            Connect();

            var res = _cache.GeoDistance(cacheKey, member1, member2, GetGeoUnit(unit));
            return res;
        }

        public async Task<double?> GeoDistanceAsync(string cacheKey, string member1, string member2, string unit = "m")
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNullOrWhiteSpace(member1, nameof(member1));
            Check.NotNullOrWhiteSpace(member2, nameof(member2));
            Check.NotNullOrWhiteSpace(unit, nameof(unit));

            await ConnectAsync();

            var res = await _cacheAsync.GeoDistanceAsync(cacheKey, member1, member2, GetGeoUnit(unit));
            return res;
        }

        public List<string> GeoHash(string cacheKey, List<string> members)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNull(members, nameof(members));

            Connect();

            var list = new List<RedisValue>();
            foreach (var item in members)
            {
                list.Add(item);
            }

            var res = _cache.GeoHash(cacheKey, list.ToArray());
            return res.ToList();
        }

        public async Task<List<string>> GeoHashAsync(string cacheKey, List<string> members)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNull(members, nameof(members));

            await ConnectAsync();

            var list = new List<RedisValue>();
            foreach (var item in members)
            {
                list.Add(item);
            }

            var res = await _cacheAsync.GeoHashAsync(cacheKey, list.ToArray());
            return res.ToList();
        }

        public List<(decimal longitude, decimal latitude)?> GeoPosition(string cacheKey, List<string> members)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNull(members, nameof(members));

            Connect();

            var list = new List<RedisValue>();
            foreach (var item in members)
            {
                list.Add(item);
            }

            var res = _cache.GeoPosition(cacheKey, list.ToArray());

            var tuple = new List<(decimal longitude, decimal latitude)?>();

            foreach (var item in res)
            {
                if (item.HasValue)
                {
                    tuple.Add((Convert.ToDecimal(item.Value.Longitude.ToString()), Convert.ToDecimal(item.Value.Latitude.ToString())));
                }
                else
                {
                    tuple.Add(null);
                }
            }

            return tuple;
        }

        public async Task<List<(decimal longitude, decimal latitude)?>> GeoPositionAsync(string cacheKey, List<string> members)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNull(members, nameof(members));

            await ConnectAsync();

            var list = new List<RedisValue>();
            foreach (var item in members)
            {
                list.Add(item);
            }

            var res = await _cacheAsync.GeoPositionAsync(cacheKey, list.ToArray());

            var tuple = new List<(decimal longitude, decimal latitude)?>();

            foreach (var item in res)
            {
                if (item.HasValue)
                {
                    tuple.Add((Convert.ToDecimal(item.Value.Longitude.ToString()), Convert.ToDecimal(item.Value.Latitude.ToString())));
                }
                else
                {
                    tuple.Add(null);
                }

            }

            return tuple;
        }

        public List<(string member, double? distance)> GeoRadius(string cacheKey, string member, double radius, string unit = "m", int count = -1, string order = "asc")
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNull(member, nameof(member));

            Connect();

            var res = _cache.GeoRadius(cacheKey, member, radius, GetGeoUnit(unit), count: count, order: GetGeoOrder(order), options: GeoRadiusOptions.WithDistance);

            var tuple = new List<(string member, double? distance)>();

            foreach (var item in res)
            {
                tuple.Add((item.Member, item.Distance));
            }

            return tuple;
        }

        public async Task<List<(string member, double? distance)>> GeoRadiusAsync(string cacheKey, string member, double radius, string unit = "m", int count = -1, string order = "asc")
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNull(member, nameof(member));

            await ConnectAsync();

            var res = await _cacheAsync.GeoRadiusAsync(cacheKey, member, radius, GetGeoUnit(unit), count: count, order: GetGeoOrder(order), options: GeoRadiusOptions.WithDistance);

            var tuple = new List<(string member, double? distance)>();

            foreach (var item in res)
            {
                tuple.Add((item.Member, item.Distance));
            }

            return tuple;
        }

        private GeoUnit GetGeoUnit(string unit)
        {
            GeoUnit geoUnit;
            switch (unit)
            {
                case "km":
                    geoUnit = GeoUnit.Kilometers;
                    break;
                case "ft":
                    geoUnit = GeoUnit.Feet;
                    break;
                case "mi":
                    geoUnit = GeoUnit.Miles;
                    break;
                default:
                    geoUnit = GeoUnit.Meters;
                    break;
            }
            return geoUnit;
        }
        private Order GetGeoOrder(string order)
        {
            Order geoOrder;
            switch (order)
            {
                default:
                case "asc":
                    geoOrder = Order.Ascending;
                    break;
                case "desc":
                    geoOrder = Order.Descending;
                    break;
            }
            return geoOrder;
        }
        #endregion

        #region HyperLogLog()
        public bool HyperLogLogAdd<T>(string cacheKey, List<T> values)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNull(values, nameof(values));

            Connect();

            var list = new List<RedisValue>();

            foreach (var item in values)
            {
                list.Add(_serializer.Serialize(item));
            }

            var res = _cache.HyperLogLogAdd(cacheKey, list.ToArray());
            return res;
        }

        public async Task<bool> HyperLogLogAddAsync<T>(string cacheKey, List<T> values)
        {
            Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            Check.NotNull(values, nameof(values));

            await ConnectAsync();

            var list = new List<RedisValue>();

            foreach (var item in values)
            {
                list.Add(_serializer.Serialize(item));
            }

            var res = await _cacheAsync.HyperLogLogAddAsync(cacheKey, list.ToArray());
            return res;
        }

        public long HyperLogLogLength(List<string> cacheKeys)
        {
            Check.NotNull(cacheKeys, nameof(cacheKeys));

            Connect();

            var list = new List<RedisKey>();

            foreach (var item in cacheKeys)
            {
                list.Add(item);
            }

            var res = _cache.HyperLogLogLength(list.ToArray());
            return res;
        }

        public async Task<long> HyperLogLogLengthAsync(List<string> cacheKeys)
        {
            Check.NotNull(cacheKeys, nameof(cacheKeys));

            await ConnectAsync();

            var list = new List<RedisKey>();

            foreach (var item in cacheKeys)
            {
                list.Add(item);
            }

            var res = await _cacheAsync.HyperLogLogLengthAsync(list.ToArray());
            return res;
        }

        public bool HyperLogLogMerge(string destKey, List<string> sourceKeys)
        {
            Check.NotNullOrWhiteSpace(destKey, nameof(destKey));
            Check.NotNull(sourceKeys, nameof(sourceKeys));

            Connect();

            var list = new List<RedisKey>();

            foreach (var item in sourceKeys)
            {
                list.Add(item);
            }

            _cache.HyperLogLogMerge(destKey, list.ToArray());
            return true;
        }

        public async Task<bool> HyperLogLogMergeAsync(string destKey, List<string> sourceKeys)
        {
            Check.NotNullOrWhiteSpace(destKey, nameof(destKey));
            Check.NotNull(sourceKeys, nameof(sourceKeys));

            await ConnectAsync();

            var list = new List<RedisKey>();

            foreach (var item in sourceKeys)
            {
                list.Add(item);
            }

            await _cacheAsync.HyperLogLogMergeAsync(destKey, list.ToArray());
            return true;
        }
        #endregion
    }
}
