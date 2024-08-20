using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Findx.Common;
using StackExchange.Redis;

namespace Findx.Redis.StackExchangeRedis;

public class RedisClient : IRedisClient
{
    private readonly IDatabase _cache;
    private readonly ConnectionMultiplexer _connection;
    private readonly IRedisSerializer _serializer;

    public RedisClient(IRedisSerializer serializer, ConnectionMultiplexer connection, string name)
    {
        _serializer = serializer;
        _connection = connection;
        _cache = _connection.GetDatabase();
        Name = name;
    }

    public string Name { get; }

    #region Public

    /// <summary>
    ///     清除key
    /// </summary>
    public void FlushDb()
    {
        var endPoints = _cache.Multiplexer.GetEndPoints();

        foreach (var endPoint in endPoints) _cache.Multiplexer.GetServer(endPoint).FlushDatabase(_cache.Database);
    }

    /// <summary>
    ///     清除key
    /// </summary>
    public async Task FlushDbAsync()
    {
        var endPoints = _cache.Multiplexer.GetEndPoints();

        foreach (var endpoint in endPoints)
            await _cache.Multiplexer.GetServer(endpoint).FlushDatabaseAsync(_cache.Database);
    }

    /// <summary>
    ///     清除当前db的所有数据
    /// </summary>
    public void Clear()
    {
        DeleteKeyWithKeyPrefix("*");
    }

    /// <summary>
    ///     清除当前db的所有数据
    /// </summary>
    public Task ClearAsync()
    {
        return DeleteKeyWithKeyPrefixAsync("*");
    }

    /// <summary>
    ///     执行redis命令
    /// </summary>
    /// <param name="script"></param>
    /// <param name="cacheKey"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public object Eval(string script, string cacheKey, IEnumerable<object> args)
    {
        Check.NotNullOrWhiteSpace(script, nameof(script));
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var redisKey = new RedisKey[] { cacheKey };

        var redisValues = new List<RedisValue>();

        foreach (var item in args)
            if (item.GetType() == typeof(byte[]))
                redisValues.Add((byte[])item);
            else
                redisValues.Add(item.ToString());

        var res = _cache.ScriptEvaluate(script, redisKey, redisValues.ToArray());

        return res;
    }

    /// <summary>
    ///     执行redis命令
    /// </summary>
    /// <param name="script"></param>
    /// <param name="cacheKey"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public async Task<object> EvalAsync(string script, string cacheKey, IEnumerable<object> args)
    {
        Check.NotNullOrWhiteSpace(script, nameof(script));
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));


        var redisKey = new RedisKey[] { cacheKey };

        var redisValues = new List<RedisValue>();

        foreach (var item in args)
            if (item.GetType() == typeof(byte[]))
                redisValues.Add((byte[])item);
            else
                redisValues.Add(item.ToString());

        var res = await _cache.ScriptEvaluateAsync(script, redisKey, redisValues.ToArray());

        return res;
    }

    #endregion Public

    #region Keys

    /// <summary>
    ///     查找当前命名前缀下共有多少个Key
    /// </summary>
    /// <returns></returns>
    public int KeyCount()
    {
        return CalcuteKeyCount("*");
    }

    /// <summary>
    ///     查找键名
    /// </summary>
    /// <param name="pattern">匹配项</param>
    /// <returns>匹配上的所有键名</returns>
    public IEnumerable<string> SearchKeys(string pattern)
    {
        var endpoints = _connection?.GetEndPoints();

        if (endpoints == null || !endpoints.Any())
            return null;

        return _connection.GetServer(endpoints.First()).Keys(pattern: pattern).Select(r => (string)r);
    }

    /// <summary>
    ///     查看缓存时间
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public long Ttl(string key)
    {
        var ts = _cache.KeyTimeToLive(key);
        return ts.HasValue ? (long)ts.Value.TotalSeconds : -1;
    }

    /// <summary>
    ///     查看缓存时间
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<long> TtlAsync(string key)
    {
        var ts = await _cache.KeyTimeToLiveAsync(key);
        return ts.HasValue ? (long)ts.Value.TotalSeconds : -1;
    }

    /// <summary>
    ///     判断是否存在当前的Key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Exists(string key)
    {
        return _cache.KeyExists(key);
    }

    /// <summary>
    ///     判断是否存在当前的Key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<bool> ExistsAsync(string key)
    {
        return await _cache.KeyExistsAsync(key);
    }

    /// <summary>
    ///     设置key的失效时间
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    public bool Expire(string key, TimeSpan expiry)
    {
        return _cache.KeyExpire(key, expiry);
    }

    /// <summary>
    ///     设置key的失效时间
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    public async Task<bool> ExpireAsync(string key, TimeSpan expiry)
    {
        return await _cache.KeyExpireAsync(key, expiry);
    }

    /// <summary>
    ///     设置key的失效时间
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    public bool Expire(string key, DateTime expiry)
    {
        return _cache.KeyExpire(key, expiry);
    }

    /// <summary>
    ///     设置key的失效时间
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    public async Task<bool> ExpireAsync(string key, DateTime expiry)
    {
        return await _cache.KeyExpireAsync(key, expiry);
    }

    /// <summary>
    ///     移除当前key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Remove(string key)
    {
        return _cache.KeyDelete(key);
    }

    /// <summary>
    ///     移除当前key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<bool> RemoveAsync(string key)
    {
        return await _cache.KeyDeleteAsync(key);
    }

    /// <summary>
    ///     移除全部key
    /// </summary>
    /// <param name="keys"></param>
    public void RemoveAll(IEnumerable<string> keys)
    {
        foreach (var key in keys) Remove(key);
    }

    /// <summary>
    ///     移除全部key
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    public async Task RemoveAllAsync(IEnumerable<string> keys)
    {
        foreach (var key in keys) await RemoveAsync(key);
    }

    /// <summary>
    ///     计算当前prefix开头的key总数
    /// </summary>
    /// <param name="prefix">key前缀</param>
    /// <returns></returns>
    public int CalcuteKeyCount(string prefix)
    {
        var retVal = _cache.ScriptEvaluate("return table.getn(redis.call('keys', ARGV[1]))",
            values: new RedisValue[] { prefix });
        if (retVal.IsNull) return 0;
        return (int)retVal;
    }

    /// <summary>
    ///     删除以当前prefix开头的所有key缓存
    /// </summary>
    /// <param name="prefix">key前缀</param>
    public void DeleteKeyWithKeyPrefix(string prefix)
    {
        _cache.ScriptEvaluate(@"
                local keys = redis.call('keys', ARGV[1])
                for i=1,#keys,5000 do
                redis.call('del', unpack(keys, i, math.min(i+4999, #keys)))
                end", values: new RedisValue[] { prefix });
    }

    /// <summary>
    ///     删除以当前prefix开头的所有key缓存
    /// </summary>
    /// <param name="prefix">key前缀</param>
    public async Task DeleteKeyWithKeyPrefixAsync(string prefix)
    {
        await _cache.ScriptEvaluateAsync(@"
                local keys = redis.call('keys', ARGV[1])
                for i=1,#keys,5000 do
                redis.call('del', unpack(keys, i, math.min(i+4999, #keys)))
                end", values: new RedisValue[] { prefix });
    }

    #endregion Keys

    #region StringSet(字符操作)

    /// <summary>
    ///     设置string键值
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">key</param>
    /// <param name="value">值</param>
    /// <param name="whenNotExists">key不存在时设置</param>
    /// <returns>成功返回true</returns>
    public bool StringSet<T>(string key, T value, bool whenNotExists = false)
    {
        var objBytes = _serializer.Serialize(value);
        if (whenNotExists)
            _cache.StringSet(key, objBytes, when: When.NotExists);
        return _cache.StringSet(key, objBytes);
    }

    /// <summary>
    ///     设置string键值
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">key</param>
    /// <param name="value">值</param>
    /// <param name="whenNotExists">key不存在时设置</param>
    /// <param name="cancellationToken"></param>
    /// <returns>成功返回true</returns>
    public async Task<bool> StringSetAsync<T>(string key, T value, bool whenNotExists = false, CancellationToken cancellationToken = default)
    {
        var objBytes = _serializer.Serialize(value);
        if (whenNotExists)
            await _cache.StringSetAsync(key, objBytes, when: When.NotExists);
        return await _cache.StringSetAsync(key, objBytes);
    }

    /// <summary>
    ///     设置string键值
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">key</param>
    /// <param name="value">值</param>
    /// <param name="expiresIn">过期间隔</param>
    /// <param name="whenNotExists">key不存在时设置</param>
    /// <returns>成功返回true</returns>
    public bool StringSet<T>(string key, T value, TimeSpan expiresIn, bool whenNotExists = false)
    {
        var objBytes = _serializer.Serialize(value);
        if (whenNotExists)
            _cache.StringSet(key, objBytes, expiresIn, when: When.NotExists);
        return _cache.StringSet(key, objBytes, expiresIn);
    }

    /// <summary>
    ///     设置string键值
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">key</param>
    /// <param name="value">值</param>
    /// <param name="expiresIn">过期间隔</param>
    /// <param name="whenNotExists">key不存在时设置</param>
    /// <param name="cancellationToken"></param>
    /// <returns>成功返回true</returns>
    public async Task<bool> StringSetAsync<T>(string key, T value, TimeSpan expiresIn, bool whenNotExists = false, CancellationToken cancellationToken = default)
    {
        var objBytes = _serializer.Serialize(value);
        if (whenNotExists)
            await _cache.StringSetAsync(key, objBytes, expiresIn, when: When.NotExists);
        return await _cache.StringSetAsync(key, objBytes, expiresIn);
    }

    /// <summary>
    ///     设置string键值
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">key</param>
    /// <param name="value">值</param>
    /// <param name="expiresAt">过期时间</param>
    /// <param name="whenNotExists">key不存在时设置</param>
    /// <returns>成功返回true</returns>
    public bool StringSet<T>(string key, T value, DateTimeOffset expiresAt, bool whenNotExists = false)
    {
        var objBytes = _serializer.Serialize(value);
        var expiration = expiresAt.Subtract(DateTimeOffset.Now);
        if (whenNotExists)
            _cache.StringSet(key, objBytes, expiration, when: When.NotExists);
        return _cache.StringSet(key, objBytes, expiration);
    }

    /// <summary>
    ///     设置string键值
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">key</param>
    /// <param name="value">值</param>
    /// <param name="expiresAt">过期时间</param>
    /// <param name="whenNotExists">key不存在时设置</param>
    /// <param name="cancellationToken"></param>
    /// <returns>成功返回true</returns>
    public async Task<bool> StringSetAsync<T>(string key, T value, DateTimeOffset expiresAt, bool whenNotExists = false, CancellationToken cancellationToken = default)
    {
        var objBytes = _serializer.Serialize(value);
        var expiration = expiresAt.Subtract(DateTimeOffset.Now);
        if (whenNotExists)
            await _cache.StringSetAsync(key, objBytes, expiration, when: When.NotExists);
        return await _cache.StringSetAsync(key, objBytes, expiration);
    }

    /// <summary>
    ///     批量设置string键值
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="items">键值列表</param>
    /// <returns>成功返回true</returns>
    public bool StringSetAll<T>(IList<Tuple<string, T>> items)
    {
        var values = items
            .Select(m => new KeyValuePair<RedisKey, RedisValue>(m.Item1, _serializer.Serialize(m.Item2))).ToArray();

        return _cache.StringSet(values);
    }

    /// <summary>
    ///     批量设置string键值
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="items">键值列表</param>
    /// <param name="cancellationToken"></param>
    /// <returns>成功返回true</returns>
    public async Task<bool> StringSetAllAsync<T>(IList<Tuple<string, T>> items, CancellationToken cancellationToken = default)
    {
        var values = items
            .Select(m => new KeyValuePair<RedisKey, RedisValue>(m.Item1, _serializer.Serialize(m.Item2))).ToArray();

        return await _cache.StringSetAsync(values);
    }

    /// <summary>
    ///     string获取值
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">key</param>
    /// <returns></returns>
    public T StringGet<T>(string key)
    {
        var valuesBytes = _cache.StringGet(key);
        if (!valuesBytes.HasValue) return default;
        return _serializer.Deserialize<T>(valuesBytes);
    }

    /// <summary>
    ///     string获取值
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">key</param>
    /// <param name="cancellationToken"></param>
    public async Task<T> StringGetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var valuesBytes = await _cache.StringGetAsync(key);
        if (!valuesBytes.HasValue) return default;
        return _serializer.Deserialize<T>(valuesBytes);
    }

    /// <summary>
    ///     键值累加
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">增长数量</param>
    /// <returns>累加后的值</returns>
    public long StringIncrement(string key, long value = 1)
    {
        return _cache.StringIncrement(key, value);
    }

    /// <summary>
    ///     键值累加
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">增长数量</param>
    /// <param name="cancellationToken"></param>
    /// <returns>累加后的值</returns>
    public async Task<long> StringIncrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
    {
        return await _cache.StringIncrementAsync(key, value);
    }

    /// <summary>
    ///     键值累加
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">增长数量</param>
    /// <returns>累加后的值</returns>
    public double StringIncrementDouble(string key, double value)
    {
        return _cache.StringIncrement(key, value);
    }

    /// <summary>
    ///     键值累加
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">增长数量</param>
    /// <param name="cancellationToken"></param>
    /// <returns>累加后的值</returns>
    public async Task<double> StringIncrementDoubleAsync(string key, double value, CancellationToken cancellationToken = default)
    {
        return await _cache.StringIncrementAsync(key, value);
    }

    /// <summary>
    ///     键值递减
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">减少数量</param>
    /// <returns>递减后的值</returns>
    public long StringDecrement(string key, long value = 1)
    {
        return _cache.StringDecrement(key, value);
    }

    /// <summary>
    ///     键值递减
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">减少数量</param>
    /// <param name="cancellationToken"></param>
    /// <returns>递减后的值</returns>
    public async Task<long> StringDecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
    {
        return await _cache.StringDecrementAsync(key, value);
    }

    /// <summary>
    ///     键值递减
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">减少数量</param>
    /// <returns>递减后的值</returns>
    public double StringDecrementDouble(string key, double value)
    {
        return _cache.StringDecrement(key, value);
    }

    /// <summary>
    ///     键值递减
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">减少数量</param>
    /// <param name="cancellationToken"></param>
    /// <returns>递减后的值</returns>
    public async Task<double> StringDecrementDoubleAsync(string key, double value, CancellationToken cancellationToken = default)
    {
        return await _cache.StringDecrementAsync(key, value);
    }

    #endregion StringSet

    #region Hash(哈希操作)

    /// <summary>
    ///     获取所有的Hash键
    /// </summary>
    /// <param name="key">key</param>
    /// <returns></returns>
    public IEnumerable<string> HashKeys(string key)
    {
        return _cache.HashKeys(key).Select(x => x.ToString());
    }

    /// <summary>
    ///     获取hash键的个数
    /// </summary>
    /// <param name="key">key</param>
    /// <returns></returns>
    public long HashLength(string key)
    {
        return _cache.HashLength(key);
    }

    /// <summary>
    ///     设置一个hash值
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">key</param>
    /// <param name="hashField">hash的键值</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    public bool HashSet<T>(string key, string hashField, T value)
    {
        return _cache.HashSet(key, hashField, _serializer.Serialize(value));
    }

    /// <summary>
    ///     设置一个hash值
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">key</param>
    /// <param name="hashField">hash的键值</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> HashSetAsync<T>(string key, string hashField, T value, CancellationToken cancellationToken = default)
    {
        var objBytes = _serializer.Serialize(value);
        return await _cache.HashSetAsync(key, hashField, objBytes);
    }

    /// <summary>
    ///     批量设置hash值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">key</param>
    /// <param name="values">键值对</param>
    public void HashSet<T>(string key, Dictionary<string, T> values)
    {
        var entries = values.Select(kv => new HashEntry(kv.Key, _serializer.Serialize(kv.Value)));

        _cache.HashSet(key, entries.ToArray());
    }

    /// <summary>
    ///     批量设置hash值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">key</param>
    /// <param name="values">键值对</param>
    /// <param name="cancellationToken"></param>
    public async Task HashSetAsync<T>(string key, Dictionary<string, T> values, CancellationToken cancellationToken = default)
    {
        var entries = values.Select(kv => new HashEntry(kv.Key, _serializer.Serialize(kv.Value)));
        await _cache.HashSetAsync(key, entries.ToArray());
    }

    /// <summary>
    ///     获取一个hash值
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <returns></returns>
    public T HashGet<T>(string key, string hashField)
    {
        var redisValue = _cache.HashGet(key, hashField);

        return redisValue.HasValue ? _serializer.Deserialize<T>(redisValue) : default;
    }

    /// <summary>
    ///     获取一个hash值
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<T> HashGetAsync<T>(string key, string hashField, CancellationToken cancellationToken = default)
    {
        var redisValue = await _cache.HashGetAsync(key, hashField);

        return redisValue.HasValue ? _serializer.Deserialize<T>(redisValue) : default;
    }

    /// <summary>
    ///     获取hash值
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
    ///     获取hash值
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">key</param>
    /// <param name="hashFields">hash键组合</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, T>> HashGetAsync<T>(string key, IEnumerable<string> hashFields, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, T>();
        foreach (var hashField in hashFields)
        {
            var value = await HashGetAsync<T>(key, hashField, cancellationToken);
            result.Add(key, value);
        }

        return result;
    }

    /// <summary>
    ///     获取全部hash值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">key</param>
    /// <returns></returns>
    public Dictionary<string, T> HashGetAll<T>(string key)
    {
        return _cache.HashGetAll(key)
            .ToDictionary(x => x.Name.ToString(), x => _serializer.Deserialize<T>(x.Value), StringComparer.Ordinal);
    }

    /// <summary>
    ///     获取全部hash值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">key</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, T>> HashGetAllAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        return (await _cache.HashGetAllAsync(key))
            .ToDictionary(x => x.Name.ToString(), x => _serializer.Deserialize<T>(x.Value), StringComparer.Ordinal);
    }

    /// <summary>
    ///     获取全部hash值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">key</param>
    /// <returns></returns>
    public IEnumerable<T> HashValues<T>(string key)
    {
        return _cache.HashValues(key).Select(m => _serializer.Deserialize<T>(m));
    }

    /// <summary>
    ///     获取全部hash值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">key</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<T>> HashValuesAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        return (await _cache.HashValuesAsync(key)).Select(m => _serializer.Deserialize<T>(m));
    }

    /// <summary>
    ///     判断是否存在hash键
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <returns></returns>
    public bool HashExists(string key, string hashField)
    {
        return _cache.HashExists(key, hashField);
    }

    /// <summary>
    ///     判断是否存在hash键
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> HashExistsAsync(string key, string hashField, CancellationToken cancellationToken = default)
    {
        return await _cache.HashExistsAsync(key, hashField);
    }

    /// <summary>
    ///     删除一个hash键
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <returns></returns>
    public bool HashDelete(string key, string hashField)
    {
        return _cache.HashDelete(key, hashField);
    }

    /// <summary>
    ///     删除一个hash键
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> HashDeleteAsync(string key, string hashField, CancellationToken cancellationToken = default)
    {
        return await _cache.HashDeleteAsync(key, hashField);
    }

    /// <summary>
    ///     删除hash键
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashFields">hash键集合</param>
    /// <returns></returns>
    public long HashDelete(string key, IEnumerable<string> hashFields)
    {
        return _cache.HashDelete(key, hashFields.Select(x => (RedisValue)x).ToArray());
    }

    /// <summary>
    ///     删除hash键
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashFields">hash键集合</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<long> HashDeleteAsync(string key, IEnumerable<string> hashFields, CancellationToken cancellationToken = default)
    {
        return await _cache.HashDeleteAsync(key, hashFields.Select(x => (RedisValue)x).ToArray());
    }

    /// <summary>
    ///     hash递增
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="value">递增值</param>
    /// <returns></returns>
    public long HashIncrement(string key, string hashField, long value = 1)
    {
        return _cache.HashIncrement(key, hashField, value);
    }

    /// <summary>
    ///     hash递增
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="value">递增值</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<long> HashIncrementAsync(string key, string hashField, long value = 1, CancellationToken cancellationToken = default)
    {
        return await _cache.HashIncrementAsync(key, hashField, value);
    }

    /// <summary>
    ///     hash递减
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="value">递减值</param>
    /// <returns></returns>
    public long HashDecrement(string key, string hashField, long value = 1)
    {
        return _cache.HashDecrement(key, hashField, value);
    }

    /// <summary>
    ///     hash递减
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="value">递减值</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<long> HashDecrementAsync(string key, string hashField, long value = 1, CancellationToken cancellationToken = default)
    {
        return await _cache.HashDecrementAsync(key, hashField, value);
    }

    /// <summary>
    ///     hash递增
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="value">递增值</param>
    /// <returns></returns>
    public double HashIncrementDouble(string key, string hashField, double value)
    {
        return _cache.HashIncrement(key, hashField, value);
    }

    /// <summary>
    ///     hash递增
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="value">递增值</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<double> HashIncrementDoubleAsync(string key, string hashField, double value, CancellationToken cancellationToken = default)
    {
        return await _cache.HashIncrementAsync(key, hashField, value);
    }

    /// <summary>
    ///     hash递减
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="value">递减值</param>
    /// <returns></returns>
    public double HashDecrementDouble(string key, string hashField, double value)
    {
        return _cache.HashDecrement(key, hashField, value);
    }

    /// <summary>
    ///     hash递减
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="value">递减值</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<double> HashDecrementDoubleAsync(string key, string hashField, double value, CancellationToken cancellationToken = default)
    {
        return await _cache.HashDecrementAsync(key, hashField, value);
    }

    #endregion hash

    #region Lock(锁操作)

    /// <summary>
    ///     获取一个锁
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">键名</param>
    /// <param name="value">值</param>
    /// <param name="expiry">过期时间</param>
    /// <returns>成功返回true</returns>
    public bool LockTake<T>(string key, T value, TimeSpan expiry)
    {
        var objBytes = _serializer.Serialize(value);
        return _cache.LockTake(key, objBytes, expiry);
    }

    /// <summary>
    ///     异步获取一个锁
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">键名</param>
    /// <param name="value">值</param>
    /// <param name="expiry">过期时间</param>
    /// <param name="cancellationToken"></param>
    /// <returns>成功返回true</returns>
    public async Task<bool> LockTakeAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var objBytes = _serializer.Serialize(value);
        return await _cache.LockTakeAsync(key, objBytes, expiry);
    }

    /// <summary>
    ///     释放一个锁
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">键名</param>
    /// <param name="value">值</param>
    /// <returns>成功返回true</returns>
    public bool LockRelease<T>(string key, T value)
    {
        var objBytes = _serializer.Serialize(value);
        return _cache.LockRelease(key, objBytes);
    }

    /// <summary>
    ///     异步释放一个锁
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">键名</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken"></param>
    /// <returns>成功返回true</returns>
    public async Task<bool> LockReleaseAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        var objBytes = _serializer.Serialize(value);
        return await _cache.LockReleaseAsync(key, objBytes);
    }

    #endregion lock

    #region List(集合操作)

    public T ListGetByIndex<T>(string cacheKey, long index)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _cache.ListGetByIndex(cacheKey, index);
        return _serializer.Deserialize<T>(bytes);
    }

    public long ListLength(string cacheKey)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        return _cache.ListLength(cacheKey);
    }

    public T ListLeftPop<T>(string cacheKey)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _cache.ListLeftPop(cacheKey);
        return _serializer.Deserialize<T>(bytes);
    }

    public long ListLeftPush<T>(string cacheKey, IList<T> cacheValues)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(cacheValues, nameof(cacheValues));

        var len = _cache.ListLeftPush(cacheKey,
            cacheValues.Select(x => (RedisValue)_serializer.Serialize(x)).ToArray());
        return len;
    }

    public IEnumerable<T> ListRange<T>(string cacheKey, long start, long stop)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _cache.ListRange(cacheKey, start, stop);
        foreach (var item in bytes)
            yield return _serializer.Deserialize<T>(item);
    }

    public long ListRemove<T>(string cacheKey, long count, T cacheValue)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);
        return _cache.ListRemove(cacheKey, bytes, count);
    }

    public bool ListSetByIndex<T>(string cacheKey, long index, T cacheValue)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);
        _cache.ListSetByIndex(cacheKey, index, bytes);
        return true;
    }

    public bool ListTrim(string cacheKey, long start, long stop)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        _cache.ListTrim(cacheKey, start, stop);
        return true;
    }

    public long ListLeftPush<T>(string cacheKey, T cacheValue)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);
        return _cache.ListLeftPush(cacheKey, bytes);
    }

    public long ListInsertBefore<T>(string cacheKey, T pivot, T cacheValue)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var pivotBytes = _serializer.Serialize(pivot);
        var cacheValueBytes = _serializer.Serialize(cacheValue);
        return _cache.ListInsertBefore(cacheKey, pivotBytes, cacheValueBytes);
    }

    public long ListInsertAfter<T>(string cacheKey, T pivot, T cacheValue)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var pivotBytes = _serializer.Serialize(pivot);
        var cacheValueBytes = _serializer.Serialize(cacheValue);
        return _cache.ListInsertAfter(cacheKey, pivotBytes, cacheValueBytes);
    }

    public long ListRightPush<T>(string cacheKey, T cacheValue)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);
        return _cache.ListRightPush(cacheKey, bytes);
    }

    public long ListRightPush<T>(string cacheKey, IList<T> cacheValues)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(cacheValues, nameof(cacheValues));

        var len = _cache.ListRightPush(cacheKey,
            cacheValues.Select(x => (RedisValue)_serializer.Serialize(x)).ToArray());
        return len;
    }

    public T ListRightPop<T>(string cacheKey)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _cache.ListRightPop(cacheKey);
        return _serializer.Deserialize<T>(bytes);
    }

    public async Task<T> ListGetByIndexAsync<T>(string cacheKey, long index, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = await _cache.ListGetByIndexAsync(cacheKey, index);
        return _serializer.Deserialize<T>(bytes);
    }

    public async Task<long> ListLengthAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        return await _cache.ListLengthAsync(cacheKey);
    }

    public async Task<T> ListLeftPopAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = await _cache.ListLeftPopAsync(cacheKey);
        return _serializer.Deserialize<T>(bytes);
    }

    public async Task<long> ListLeftPushAsync<T>(string cacheKey, T cacheValue, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);
        return await _cache.ListLeftPushAsync(cacheKey, bytes);
    }

    public async Task<long> ListLeftPushAsync<T>(string cacheKey, IList<T> cacheValues, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(cacheValues, nameof(cacheValues));

        var len = await _cache.ListLeftPushAsync(cacheKey,
            cacheValues.Select(x => (RedisValue)_serializer.Serialize(x)).ToArray());
        return len;
    }

    public async Task<T> ListRightPopAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = await _cache.ListRightPopAsync(cacheKey);
        return _serializer.Deserialize<T>(bytes);
    }

    public async Task<long> ListRightPushAsync<T>(string cacheKey, T cacheValue, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);
        return await _cache.ListRightPushAsync(cacheKey, bytes);
    }

    public async Task<long> ListRightPushAsync<T>(string cacheKey, IList<T> cacheValues, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(cacheValues, nameof(cacheValues));

        var len = await _cache.ListRightPushAsync(cacheKey,
            cacheValues.Select(x => (RedisValue)_serializer.Serialize(x)).ToArray());
        return len;
    }

    public async Task<IEnumerable<T>> ListRangeAsync<T>(string cacheKey, long start, long stop, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var list = new List<T>();

        var bytes = await _cache.ListRangeAsync(cacheKey, start, stop);

        foreach (var item in bytes) list.Add(_serializer.Deserialize<T>(item));

        return list;
    }

    public async Task<long> ListRemoveAsync<T>(string cacheKey, long count, T cacheValue, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);
        return await _cache.ListRemoveAsync(cacheKey, bytes, count);
    }

    public async Task<bool> ListSetByIndexAsync<T>(string cacheKey, long index, T cacheValue, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);
        await _cache.ListSetByIndexAsync(cacheKey, index, bytes);
        return true;
    }

    public async Task<bool> ListTrimAsync(string cacheKey, long start, long stop, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        await _cache.ListTrimAsync(cacheKey, start, stop);
        return true;
    }

    public async Task<long> ListInsertBeforeAsync<T>(string cacheKey, T pivot, T cacheValue, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var pivotBytes = _serializer.Serialize(pivot);
        var cacheValueBytes = _serializer.Serialize(cacheValue);
        return await _cache.ListInsertBeforeAsync(cacheKey, pivotBytes, cacheValueBytes);
    }

    public async Task<long> ListInsertAfterAsync<T>(string cacheKey, T pivot, T cacheValue, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var pivotBytes = _serializer.Serialize(pivot);
        var cacheValueBytes = _serializer.Serialize(cacheValue);
        return await _cache.ListInsertAfterAsync(cacheKey, pivotBytes, cacheValueBytes);
    }

    #endregion

    #region Set(数组操作)

    public long SetAdd<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(cacheValues, nameof(cacheValues));

        var len = _cache.SetAdd(cacheKey, cacheValues.Select(x => (RedisValue)_serializer.Serialize(x)).ToArray());

        if (expiration.HasValue) _cache.KeyExpire(cacheKey, expiration.Value);

        return len;
    }

    public long SetLength(string cacheKey)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var len = _cache.SetLength(cacheKey);
        return len;
    }

    public bool SetContains<T>(string cacheKey, T cacheValue)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);

        var flag = _cache.SetContains(cacheKey, bytes);
        return flag;
    }

    public IEnumerable<T> SetMembers<T>(string cacheKey)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            
        var bytes = _cache.SetMembers(cacheKey);
        foreach (var item in bytes) 
            yield return _serializer.Deserialize<T>(item);
    }

    public T SetPop<T>(string cacheKey)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _cache.SetPop(cacheKey);
        return _serializer.Deserialize<T>(bytes);
    }

    public IEnumerable<T> SetRandomMembers<T>(string cacheKey, int count = 1)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            
        var bytes = _cache.SetRandomMembers(cacheKey, count);
        foreach (var item in bytes) yield 
            return _serializer.Deserialize<T>(item);
    }

    public long SetRemove<T>(string cacheKey, IList<T> cacheValues = null)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        long len;

        if (cacheValues != null && cacheValues.Any())
        {
            len = _cache.SetRemove(cacheKey,
                cacheValues.Select(x => (RedisValue)_serializer.Serialize(x)).ToArray());
        }
        else
        {
            var flag = _cache.KeyDelete(cacheKey);
            len = flag ? 1 : 0;
        }

        return len;
    }

    public async Task<long> SetAddAsync<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(cacheValues, nameof(cacheValues));

        var len = await _cache.SetAddAsync(cacheKey,
            cacheValues.Select(x => (RedisValue)_serializer.Serialize(x)).ToArray());

        if (expiration.HasValue) await _cache.KeyExpireAsync(cacheKey, expiration.Value);

        return len;
    }

    public async Task<long> SetLengthAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var len = await _cache.SetLengthAsync(cacheKey);
        return len;
    }

    public async Task<bool> SetContainsAsync<T>(string cacheKey, T cacheValue, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);

        var flag = await _cache.SetContainsAsync(cacheKey, bytes);
        return flag;
    }

    public async Task<IEnumerable<T>> SetMembersAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var list = new List<T>();

        var vals = await _cache.SetMembersAsync(cacheKey);

        foreach (var item in vals) list.Add(_serializer.Deserialize<T>(item));

        return list;
    }

    public async Task<T> SetPopAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = await _cache.SetPopAsync(cacheKey);

        return _serializer.Deserialize<T>(bytes);
    }

    public async Task<IEnumerable<T>> SetRandomMembersAsync<T>(string cacheKey, int count = 1, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var list = new List<T>();

        var bytes = await _cache.SetRandomMembersAsync(cacheKey, count);

        foreach (var item in bytes) list.Add(_serializer.Deserialize<T>(item));

        return list;
    }

    public async Task<long> SetRemoveAsync<T>(string cacheKey, IList<T> cacheValues = null, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        long len;
        if (cacheValues != null && cacheValues.Any())
        {
            len = await _cache.SetRemoveAsync(cacheKey,
                cacheValues.Select(x => (RedisValue)_serializer.Serialize(x)).ToArray());
        }
        else
        {
            var flag = await _cache.KeyDeleteAsync(cacheKey);
            len = flag ? 1 : 0;
        }

        return len;
    }

    #endregion

    #region Sorted Set(有序数组)

    public long SortedSetAdd<T>(string cacheKey, Dictionary<T, double> cacheValues)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var len = _cache.SortedSetAdd(cacheKey,
            cacheValues.Select(x => new SortedSetEntry(_serializer.Serialize(x.Key), x.Value)).ToArray());

        return len;
    }

    public long SortedSetLength(string cacheKey)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var len = _cache.SortedSetLength(cacheKey);
        return len;
    }

    public long SortedSetLengthByValue(string cacheKey, double min, double max)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var len = _cache.SortedSetLengthByValue(cacheKey, min, max);
        return len;
    }

    public double SortedSetIncrement(string cacheKey, string field, double val = 1)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNullOrWhiteSpace(field, nameof(field));

        var value = _cache.SortedSetIncrement(cacheKey, field, val);
        return value;
    }

    public long SortedSetLengthByValue(string cacheKey, string min, string max)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var len = _cache.SortedSetLengthByValue(cacheKey, min, max);
        return len;
    }

    public IEnumerable<T> SortedSetRangeByRank<T>(string cacheKey, long start, long stop)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _cache.SortedSetRangeByRank(cacheKey, start, stop);
        foreach (var item in bytes) 
            yield return _serializer.Deserialize<T>(item);
    }

    public long? SortedSetRank<T>(string cacheKey, T cacheValue)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);

        var index = _cache.SortedSetRank(cacheKey, bytes);

        return index;
    }

    public long SortedSetRemove<T>(string cacheKey, IList<T> cacheValues)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var len = _cache.SortedSetRemove(cacheKey,
            cacheValues.Select(x => (RedisValue)_serializer.Serialize(x)).ToArray());

        return len;
    }

    public double? SortedSetScore<T>(string cacheKey, T cacheValue)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);

        var score = _cache.SortedSetScore(cacheKey, bytes);

        return score;
    }

    public async Task<long> SortedSetAddAsync<T>(string cacheKey, Dictionary<T, double> cacheValues, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var len = await _cache.SortedSetAddAsync(cacheKey,
            cacheValues.Select(x => new SortedSetEntry(_serializer.Serialize(x.Key), x.Value)).ToArray());

        return len;
    }

    public async Task<long> SortedSetLengthAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var len = await _cache.SortedSetLengthAsync(cacheKey);
        return len;
    }

    public async Task<long> SortedSetLengthByValueAsync(string cacheKey, double min, double max, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var len = await _cache.SortedSetLengthByValueAsync(cacheKey, min, max);
        return len;
    }

    public async Task<double> SortedSetIncrementAsync(string cacheKey, string field, double val = 1, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNullOrWhiteSpace(field, nameof(field));

        var value = await _cache.SortedSetIncrementAsync(cacheKey, field, val);
        return value;
    }

    public async Task<long> SortedSetLengthByValueAsync(string cacheKey, string min, string max, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var len = await _cache.SortedSetLengthByValueAsync(cacheKey, min, max);
        return len;
    }

    public async Task<IEnumerable<T>> SortedSetRangeByRankAsync<T>(string cacheKey, long start, long stop, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var list = new List<T>();

        var bytes = await _cache.SortedSetRangeByRankAsync(cacheKey, start, stop);

        foreach (var item in bytes) list.Add(_serializer.Deserialize<T>(item));

        return list;
    }

    public async Task<long?> SortedSetRankAsync<T>(string cacheKey, T cacheValue, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);

        var index = await _cache.SortedSetRankAsync(cacheKey, bytes);

        return index;
    }

    public async Task<long> SortedSetRemoveAsync<T>(string cacheKey, IList<T> cacheValues, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = new List<RedisValue>();

        foreach (var item in cacheValues) bytes.Add(_serializer.Serialize(item));

        var len = await _cache.SortedSetRemoveAsync(cacheKey, bytes.ToArray());

        return len;
    }

    public async Task<double?> SortedSetScoreAsync<T>(string cacheKey, T cacheValue, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);

        var score = await _cache.SortedSetScoreAsync(cacheKey, bytes);

        return score;
    }

    #endregion

    #region Geo(经纬度操作)

    public long GeoAdd(string cacheKey, IEnumerable<(double longitude, double latitude, string member)> values)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(values, nameof(values));

        var res = _cache.GeoAdd(cacheKey,
            values.Select(x => new GeoEntry(x.longitude, x.latitude, x.member)).ToArray());
        return res;
    }

    public async Task<long> GeoAddAsync(string cacheKey,
        IEnumerable<(double longitude, double latitude, string member)> values, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(values, nameof(values));

        var res = await _cache.GeoAddAsync(cacheKey,
            values.Select(x => new GeoEntry(x.longitude, x.latitude, x.member)).ToArray());
        return res;
    }

    public double? GeoDistance(string cacheKey, string member1, string member2, string unit = "m")
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNullOrWhiteSpace(member1, nameof(member1));
        Check.NotNullOrWhiteSpace(member2, nameof(member2));
        Check.NotNullOrWhiteSpace(unit, nameof(unit));

        var res = _cache.GeoDistance(cacheKey, member1, member2, GetGeoUnit(unit));
        return res;
    }

    public async Task<double?> GeoDistanceAsync(string cacheKey, string member1, string member2, string unit = "m", CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNullOrWhiteSpace(member1, nameof(member1));
        Check.NotNullOrWhiteSpace(member2, nameof(member2));
        Check.NotNullOrWhiteSpace(unit, nameof(unit));

        var res = await _cache.GeoDistanceAsync(cacheKey, member1, member2, GetGeoUnit(unit));
        return res;
    }

    public string[] GeoHash(string cacheKey, IEnumerable<string> members)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(members, nameof(members));

        return _cache.GeoHash(cacheKey, members.Select(x => (RedisValue)x).ToArray());
    }

    public async Task<string[]> GeoHashAsync(string cacheKey, IEnumerable<string> members, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(members, nameof(members));

        return await _cache.GeoHashAsync(cacheKey, members.Select(x => (RedisValue)x).ToArray());
    }

    public IEnumerable<(decimal longitude, decimal latitude)?> GeoPosition(string cacheKey, IEnumerable<string> members)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(members, nameof(members));

        var res = _cache.GeoPosition(cacheKey, members.Select(x => (RedisValue)x).ToArray());

        foreach (var item in res)
        {
            if (item.HasValue)
                yield return (Convert.ToDecimal(item.Value.Longitude), Convert.ToDecimal(item.Value.Latitude));
            else
                yield return null;
        }
    }

    public async Task<IEnumerable<(decimal longitude, decimal latitude)?>> GeoPositionAsync(string cacheKey, IEnumerable<string> members, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(members, nameof(members));

        var res = await _cache.GeoPositionAsync(cacheKey, members.Select(x => (RedisValue)x).ToArray());

        var tuple = new List<(decimal longitude, decimal latitude)?>();

        foreach (var item in res)
            if (item.HasValue)
                tuple.Add((Convert.ToDecimal(item.Value.Longitude), Convert.ToDecimal(item.Value.Latitude)));
            else
                tuple.Add(null);

        return tuple;
    }

    public IEnumerable<(string member, double? distance)> GeoRadius(string cacheKey, string member, double radius,
        string unit = "m", int count = -1, string order = "asc")
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(member, nameof(member));

        var res = _cache.GeoRadius(cacheKey, member, radius, GetGeoUnit(unit), count, GetGeoOrder(order), GeoRadiusOptions.WithDistance);
        foreach (var item in res)
            yield return (item.Member, item.Distance);
    }

    public async Task<IEnumerable<(string member, double? distance)>> GeoRadiusAsync(string cacheKey, string member,
        double radius, string unit = "m", int count = -1, string order = "asc", CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(member, nameof(member));

        var res = await _cache.GeoRadiusAsync(cacheKey, member, radius, GetGeoUnit(unit), count, GetGeoOrder(order),
            GeoRadiusOptions.WithDistance);

        var tuple = new List<(string member, double? distance)>();

        foreach (var item in res) 
            tuple.Add((item.Member, item.Distance));

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
            case "desc":
                geoOrder = Order.Descending;
                break;
            default:
                geoOrder = Order.Ascending;
                break;
        }

        return geoOrder;
    }

    #endregion

    #region HyperLogLog()

    public bool HyperLogLogAdd<T>(string cacheKey, IEnumerable<T> values)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(values, nameof(values));

        var res = _cache.HyperLogLogAdd(cacheKey,
            values.Select(x => (RedisValue)_serializer.Serialize(x)).ToArray());
        return res;
    }

    public async Task<bool> HyperLogLogAddAsync<T>(string cacheKey, IEnumerable<T> values, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(values, nameof(values));

        var res = await _cache.HyperLogLogAddAsync(cacheKey,
            values.Select(x => (RedisValue)_serializer.Serialize(x)).ToArray());
        return res;
    }

    public long HyperLogLogLength(IEnumerable<string> cacheKeys)
    {
        Check.NotNull(cacheKeys, nameof(cacheKeys));

        var res = _cache.HyperLogLogLength(cacheKeys.Select(x => (RedisKey)x).ToArray());
        return res;
    }

    public async Task<long> HyperLogLogLengthAsync(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default)
    {
        Check.NotNull(cacheKeys, nameof(cacheKeys));

        var res = await _cache.HyperLogLogLengthAsync(cacheKeys.Select(x => (RedisKey)x).ToArray());
        return res;
    }

    public bool HyperLogLogMerge(string destKey, IEnumerable<string> sourceKeys)
    {
        Check.NotNullOrWhiteSpace(destKey, nameof(destKey));
        Check.NotNull(sourceKeys, nameof(sourceKeys));

        _cache.HyperLogLogMerge(destKey, sourceKeys.Select(x => (RedisKey)x).ToArray());
        return true;
    }

    public async Task<bool> HyperLogLogMergeAsync(string destKey, IEnumerable<string> sourceKeys, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(destKey, nameof(destKey));
        Check.NotNull(sourceKeys, nameof(sourceKeys));

        await _cache.HyperLogLogMergeAsync(destKey, sourceKeys.Select(x => (RedisKey)x).ToArray());
        return true;
    }

    #endregion
}