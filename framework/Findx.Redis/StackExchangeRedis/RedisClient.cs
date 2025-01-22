using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Findx.Common;
using StackExchange.Redis;
using When = Findx.Common.When;

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
    
    #region Keys
    
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

    /// <summary>
    ///     查找键名
    /// </summary>
    /// <param name="pattern">匹配项</param>
    /// <param name="count"></param>
    /// <returns>匹配上的所有键名</returns>
    public IEnumerable<string> SearchKeys(string pattern, int? count)
    {
        var endpoints = _connection?.GetEndPoints();

        if (_connection == null || endpoints == null || endpoints.Length == 0) 
            yield break;
        
        var server = _connection.GetServer(endpoints[0]);
        var keys = server.Keys(pattern: pattern, pageSize: count ?? 250);
        foreach (var key in keys)
        {
            yield return key;
        }
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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<long> TtlAsync(string key, CancellationToken cancellationToken = default)
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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> ExpireAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default)
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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _cache.KeyDeleteAsync(key);
    }

    /// <summary>
    ///     移除全部key
    /// </summary>
    /// <param name="keys"></param>
    public long RemoveAll(IEnumerable<string> keys)
    {
        return _cache.KeyDelete(keys.Select(x => new RedisKey(x)).ToArray());
    }

    /// <summary>
    ///     移除全部key
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<long> RemoveAllAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        return await _cache.KeyDeleteAsync(keys.Select(x => new RedisKey(x)).ToArray());
    }
    
    #endregion Keys

    #region StringSet(字符操作)

    private StackExchange.Redis.When ConvertToWhen(When when)
    {
        switch (when)
        {
            default:
            case When.Always:
                return StackExchange.Redis.When.Always;
            case When.Exists:
                return StackExchange.Redis.When.Exists;
            case When.NotExists:
                return StackExchange.Redis.When.NotExists;
        }
    }
    
    /// <summary>
    ///     设置string键值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="value">值</param>
    /// <param name="expiration"></param>
    /// <param name="when">key不存在时设置</param>
    /// <returns>成功返回true</returns>
    public bool StringSet(string key, string value, TimeSpan? expiration = null, When when = When.Always)
    {
        return _cache.StringSet(key, value, expiration, ConvertToWhen(when));
    }

    /// <summary>
    ///     设置string键值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="value">值</param>
    /// <param name="expiration"></param>
    /// <param name="when">key不存在时设置</param>
    /// <param name="cancellationToken"></param>
    /// <returns>成功返回true</returns>
    public async Task<bool> StringSetAsync(string key, string value, TimeSpan? expiration = null, When when = When.Always, CancellationToken cancellationToken = default)
    {
        return await _cache.StringSetAsync(key, value, expiration, ConvertToWhen(when));
    }
    
    /// <summary>
    ///     string获取值
    /// </summary>
    /// <param name="key">key</param>
    /// <returns></returns>
    public string StringGet(string key)
    {
        return _cache.StringGet(key);
    }

    /// <summary>
    ///     string获取值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="cancellationToken"></param>
    public async Task<string> StringGetAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _cache.StringGetAsync(key);
    }

    /// <summary>
    ///     键值累加
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">增长数量</param>
    /// <returns>累加后的值</returns>
    public long IncrBy(string key, long value = 1)
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
    public async Task<long> IncrByAsync(string key, long value = 1, CancellationToken cancellationToken = default)
    {
        return await _cache.StringIncrementAsync(key, value);
    }

    /// <summary>
    ///     键值累加
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">增长数量</param>
    /// <returns>累加后的值</returns>
    public double IncrByFloat(string key, double value)
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
    public async Task<double> IncrByFloatAsync(string key, double value, CancellationToken cancellationToken = default)
    {
        return await _cache.StringIncrementAsync(key, value);
    }

    /// <summary>
    ///     键值递减
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">减少数量</param>
    /// <returns>递减后的值</returns>
    public long DecrBy(string key, long value = 1)
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
    public async Task<long> DecrByAsync(string key, long value = 1, CancellationToken cancellationToken = default)
    {
        return await _cache.StringDecrementAsync(key, value);
    }

    /// <summary>
    ///     键值递减
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">减少数量</param>
    /// <returns>递减后的值</returns>
    public double DecrByFloat(string key, double value)
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
    public async Task<double> DecrByFloatAsync(string key, double value, CancellationToken cancellationToken = default)
    {
        return await _cache.StringDecrementAsync(key, value);
    }

    #endregion StringSet

    #region Hash(哈希操作)

    /// <summary>
    ///     设置一个hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash的键值</param>
    /// <param name="value">值</param>
    /// <param name="when"></param>
    /// <returns></returns>
    public bool HSet(string key, string hashField, string value, When when = When.Always)
    {
        return _cache.HashSet(key, hashField, value, ConvertToWhen(when));
    }

    /// <summary>
    ///     设置一个hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash的键值</param>
    /// <param name="value">值</param>
    /// <param name="when"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> HSetAsync(string key, string hashField, string value, When when = When.Always, CancellationToken cancellationToken = default)
    {
        return await _cache.HashSetAsync(key, hashField, value, ConvertToWhen(when));
    }
    
    /// <summary>
    ///     批量设置hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="values">键值对</param>
    /// <param name="expiration"></param>
    // ReSharper disable once InconsistentNaming
    public bool HMSet(string key, Dictionary<string, string> values, TimeSpan? expiration = null)
    {
        var entries = values.Select(kv => new HashEntry(kv.Key, kv.Value));
        _cache.HashSet(key, entries.ToArray());
        return !expiration.HasValue || _cache.KeyExpire(key, expiration);
    }

    /// <summary>
    ///     批量设置hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="values">键值对</param>
    /// <param name="expiration"></param>
    /// <param name="cancellationToken"></param>
    public async Task<bool> HMSetAsync(string key, Dictionary<string, string> values, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var entries = values.Select(kv => new HashEntry(kv.Key, kv.Value));
        await _cache.HashSetAsync(key, entries.ToArray());
        return !expiration.HasValue || await _cache.KeyExpireAsync(key, expiration);
    }
    
    /// <summary>
    ///     获取一个hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <returns></returns>
    public string HGet(string key, string hashField)
    {
        return _cache.HashGet(key, hashField);
    }

    /// <summary>
    ///     获取一个hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string> HGetAsync(string key, string hashField, CancellationToken cancellationToken = default)
    {
        return await _cache.HashGetAsync(key, hashField);
    }
    
    /// <summary>
    ///     获取hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashFields">hash键组合</param>
    /// <returns></returns>
    // ReSharper disable once InconsistentNaming
    public Dictionary<string, string> HMGet(string key, IList<string> hashFields)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var redisValues = _cache.HashGet(key, hashFields.Select(x => (RedisValue)x).ToArray());
        for (var i = 0; i < hashFields.Count; i++)
        {
            if (!dict.ContainsKey(hashFields[i]))
            {
                dict.Add(hashFields[i], redisValues[i]);
            }
        }
        return dict;
    }

    /// <summary>
    ///     获取hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashFields">hash键组合</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    // ReSharper disable once InconsistentNaming
    public async Task<Dictionary<string, string>> HMGetAsync(string key, IList<string> hashFields, CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var redisValues = await _cache.HashGetAsync(key, hashFields.Select(x => (RedisValue)x).ToArray());
        for (var i = 0; i < hashFields.Count; i++)
        {
            if (!dict.ContainsKey(hashFields[i]))
            {
                dict.Add(hashFields[i], redisValues[i]);
            }
        }
        return dict;
    }
    
    /// <summary>
    ///     获取全部hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <returns></returns>
    public Dictionary<string, string> HGetAll(string key)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var redisValues = _cache.HashGetAll(key);
        foreach (var redisValue in redisValues)
        {
            dict.Add(redisValue.Name, redisValue.Value);
        }
        return dict;
    }

    /// <summary>
    ///     获取全部hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, string>> HGetAllAsync(string key, CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var redisValues = await _cache.HashGetAllAsync(key);
        foreach (var redisValue in redisValues)
        {
            dict.Add(redisValue.Name, redisValue.Value);
        }
        return dict;
    }
    
    /// <summary>
    ///     获取全部hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <returns></returns>
    public IEnumerable<string> HVals(string key)
    {
        return _cache.HashValues(key).Select(x => x.ToString());
    }

    /// <summary>
    ///     获取全部hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<string>> HValsAsync(string key, CancellationToken cancellationToken = default)
    {
        return (await _cache.HashValuesAsync(key)).Select(m => m.ToString());
    }
    
    /// <summary>
    ///     获取所有的Hash键
    /// </summary>
    /// <param name="key">key</param>
    /// <returns></returns>
    public IEnumerable<string> HKeys(string key)
    {
        return _cache.HashKeys(key).Select(x => x.ToString());
    }

    /// <summary>
    ///     获取hash键的个数
    /// </summary>
    /// <param name="key">key</param>
    /// <returns></returns>
    public long HLen(string key)
    {
        return _cache.HashLength(key);
    }

    /// <summary>
    ///     判断是否存在hash键
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <returns></returns>
    public bool HExists(string key, string hashField)
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
    public async Task<bool> HExistsAsync(string key, string hashField, CancellationToken cancellationToken = default)
    {
        return await _cache.HashExistsAsync(key, hashField);
    }

    /// <summary>
    ///     删除一个hash键
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <returns></returns>
    public bool HDel(string key, string hashField)
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
    public async Task<bool> HDelAsync(string key, string hashField, CancellationToken cancellationToken = default)
    {
        return await _cache.HashDeleteAsync(key, hashField);
    }

    /// <summary>
    ///     删除hash键
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashFields">hash键集合,为空则删除整个hash</param>
    /// <returns></returns>
    public long HDel(string key, IEnumerable<string> hashFields)
    {
        if (hashFields == null || !hashFields.Any())
            return _cache.KeyDelete(key) ? 1 : 0;
        
        return _cache.HashDelete(key, hashFields.Select(x => (RedisValue)x).ToArray());
    }

    /// <summary>
    ///     删除hash键
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashFields">hash键集合,为空则删除整个hash</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<long> HDelAsync(string key, IEnumerable<string> hashFields, CancellationToken cancellationToken = default)
    {
        if (hashFields == null || !hashFields.Any())
            return await _cache.KeyDeleteAsync(key) ? 1 : 0;
        
        return await _cache.HashDeleteAsync(key, hashFields.Select(x => (RedisValue)x).ToArray());
    }

    /// <summary>
    ///     hash递增
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="value">递增值</param>
    /// <returns></returns>
    public long HIncrBy(string key, string hashField, long value = 1)
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
    public async Task<long> HIncrByAsync(string key, string hashField, long value = 1, CancellationToken cancellationToken = default)
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
    public long HDecr(string key, string hashField, long value = 1)
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
    public async Task<long> HDecrAsync(string key, string hashField, long value = 1, CancellationToken cancellationToken = default)
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
    public double HIncrByFloat(string key, string hashField, double value)
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
    public async Task<double> HIncrByFloatAsync(string key, string hashField, double value, CancellationToken cancellationToken = default)
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
    public double HDecrByFloat(string key, string hashField, double value)
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
    public async Task<double> HDecrByFloatAsync(string key, string hashField, double value, CancellationToken cancellationToken = default)
    {
        return await _cache.HashDecrementAsync(key, hashField, value);
    }

    #endregion hash

    #region Lock(锁操作)

    /// <summary>
    ///     获取一个锁
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">值</param>
    /// <param name="expiry">过期时间</param>
    /// <returns>成功返回true</returns>
    public bool Lock(string key, string value, TimeSpan expiry)
    {
        return _cache.LockTake(key, value, expiry);
    }

    /// <summary>
    ///     异步获取一个锁
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">值</param>
    /// <param name="expiry">过期时间</param>
    /// <param name="cancellationToken"></param>
    /// <returns>成功返回true</returns>
    public async Task<bool> LockAsync(string key, string value, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        return await _cache.LockTakeAsync(key, value, expiry);
    }

    /// <summary>
    ///     释放一个锁
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">值</param>
    /// <returns>成功返回true</returns>
    public bool LockRelease(string key, string value)
    {
        return _cache.LockRelease(key, value);
    }

    /// <summary>
    ///     异步释放一个锁
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken"></param>
    /// <returns>成功返回true</returns>
    public async Task<bool> LockReleaseAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        return await _cache.LockReleaseAsync(key, value);
    }

    #endregion lock

    #region List(集合操作)

    /// <summary>
    ///     获取列表中某个位置的元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="index"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T LIndex<T>(string cacheKey, long index)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _cache.ListGetByIndex(cacheKey, index);
        return _serializer.Deserialize<T>(bytes);
    }

    /// <summary>
    ///     列表中的元素个数
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <returns></returns>
    public long LLen(string cacheKey)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        return _cache.ListLength(cacheKey);
    }
    
    /// <summary>
    ///     将一个值插入到列表头部
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public long LPush<T>(string cacheKey, T cacheValue)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);
        return _cache.ListLeftPush(cacheKey, bytes);
    }

    /// <summary>
    ///     将多个值插入到列表头部
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public long LPush<T>(string cacheKey, IList<T> cacheValues)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(cacheValues, nameof(cacheValues));

        var redisValues = cacheValues.Select(item => (RedisValue)_serializer.Serialize(item)).ToArray();
        return _cache.ListLeftPush(cacheKey, redisValues);
    }
    
    /// <summary>
    ///     移出并获取列表的第一个元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T LPop<T>(string cacheKey)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _cache.ListLeftPop(cacheKey);
        return _serializer.Deserialize<T>(bytes);
    }
    
    /// <summary>
    ///     获取列表指定范围内的元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IEnumerable<T> LRange<T>(string cacheKey, long start, long stop)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _cache.ListRange(cacheKey, start, stop);
        foreach (var item in bytes) yield return _serializer.Deserialize<T>(item);
    }

    /// <summary>
    ///     根据参数 COUNT 的值，移除列表中与参数 VALUE 相等的元素。
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="count">等于0删除所有,大于0从表头开始向表尾搜索删除最多count个与value相等的项,小于0从表尾开始向表头搜索删除最多count个与value相等的项</param>
    /// <param name="cacheValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>删除的数量</returns>
    public long LRem<T>(string cacheKey, long count, T cacheValue)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);
        return _cache.ListRemove(cacheKey, bytes, count);
    }

    /// <summary>
    ///     通过索引设置列表元素的值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="index"></param>
    /// <param name="cacheValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool LSet<T>(string cacheKey, long index, T cacheValue)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);
        _cache.ListSetByIndex(cacheKey, index, bytes);
        
        return true;
    }

    /// <summary>
    ///     对一个列表进行修剪(trim)
    /// </summary>
    /// <remarks>让列表只保留指定区间内的元素，不在指定区间之内的元素都将被删除</remarks>
    /// <param name="cacheKey"></param>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    /// <returns></returns>
    public bool LTrim(string cacheKey, long start, long stop)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        _cache.ListTrim(cacheKey, start, stop);
        
        return true;
    }

    /// <summary>
    ///     在 pivot 元素前面插入一个元素 value
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="pivot"></param>
    /// <param name="cacheValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public long LInsertBefore<T>(string cacheKey, T pivot, T cacheValue)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var pivotBytes = _serializer.Serialize(pivot);
        var cacheValueBytes = _serializer.Serialize(cacheValue);
        
        return _cache.ListInsertBefore(cacheKey, pivotBytes, cacheValueBytes);
    }

    /// <summary>
    ///     在 pivot 元素的后面插入一个元素 value
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="pivot"></param>
    /// <param name="cacheValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public long LInsertAfter<T>(string cacheKey, T pivot, T cacheValue)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var pivotBytes = _serializer.Serialize(pivot);
        var cacheValueBytes = _serializer.Serialize(cacheValue);
        
        return _cache.ListInsertAfter(cacheKey, pivotBytes, cacheValueBytes);
    }

    /// <summary>
    ///     从列表的右侧插入值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public long RPush<T>(string cacheKey, T cacheValue)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);
        return _cache.ListRightPush(cacheKey, bytes);
    }

    /// <summary>
    ///     从列表的右侧插入多个值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public long RPush<T>(string cacheKey, IList<T> cacheValues)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(cacheValues, nameof(cacheValues));

        var redisValues = cacheValues.Select(item => (RedisValue)_serializer.Serialize(item)).ToArray();

        return _cache.ListRightPush(cacheKey, redisValues);
    }

    /// <summary>
    ///     移出并获取列表的最后一个元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T RPop<T>(string cacheKey)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _cache.ListRightPop(cacheKey);
        return _serializer.Deserialize<T>(bytes);
    }

    /// <summary>
    ///     获取列表中某个位置的元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="index"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<T> LIndexAsync<T>(string cacheKey, long index, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = await _cache.ListGetByIndexAsync(cacheKey, index);
        return _serializer.Deserialize<T>(bytes);
    }

    /// <summary>
    ///     列表中的元素个数
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<long> LLenAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        return await _cache.ListLengthAsync(cacheKey);
    }

    /// <summary>
    ///     从列表的左侧插入值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValue"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<long> LPushAsync<T>(string cacheKey, T cacheValue, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);
        return await _cache.ListLeftPushAsync(cacheKey, bytes);
    }

    /// <summary>
    ///     从列表的左侧插入多个值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<long> LPushAsync<T>(string cacheKey, IList<T> cacheValues, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(cacheValues, nameof(cacheValues));

        var redisValues = cacheValues.Select(item => (RedisValue)_serializer.Serialize(item)).ToArray();
        return await _cache.ListLeftPushAsync(cacheKey, redisValues);
    }
    
    /// <summary>
    ///     从列表的左侧取出一个值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<T> LPopAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = await _cache.ListLeftPopAsync(cacheKey);
        return _serializer.Deserialize<T>(bytes);
    }
    
    /// <summary>
    ///     从列表的右侧插入值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValue"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<long> RPushAsync<T>(string cacheKey, T cacheValue, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);
        return await _cache.ListRightPushAsync(cacheKey, bytes);
    }

    /// <summary>
    ///     从列表的右侧插入多个值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<long> RPushAsync<T>(string cacheKey, IList<T> cacheValues, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(cacheValues, nameof(cacheValues));

        var redisValues = cacheValues.Select(item => (RedisValue)_serializer.Serialize(item)).ToArray();

        return await _cache.ListRightPushAsync(cacheKey, redisValues);
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<T> RPopAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = await _cache.ListRightPopAsync(cacheKey);
        return _serializer.Deserialize<T>(bytes);
    }

    /// <summary>
    ///     取出列表中的值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<IEnumerable<T>> LRangeAsync<T>(string cacheKey, long start, long stop, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var list = new List<T>();

        var bytes = await _cache.ListRangeAsync(cacheKey, start, stop);

        foreach (var item in bytes) list.Add(_serializer.Deserialize<T>(item));

        return list;
    }

    /// <summary>
    ///     删除列表中的一个元素,可设置要删除的数量,返回删除的数量
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="count"></param>
    /// <param name="cacheValue"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<long> LRemAsync<T>(string cacheKey, long count, T cacheValue, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);
        return await _cache.ListRemoveAsync(cacheKey, bytes, count);
    }

    /// <summary>
    ///     设置列表中某个位置的元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="index"></param>
    /// <param name="cacheValue"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<bool> LSetAsync<T>(string cacheKey, long index, T cacheValue, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);
        await _cache.ListSetByIndexAsync(cacheKey, index, bytes);
        
        return true;
    }

    /// <summary>
    ///     按指定范围裁剪列表
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> LTrimAsync(string cacheKey, long start, long stop, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        await _cache.ListTrimAsync(cacheKey, start, stop);
        
        return true;
    }

    /// <summary>
    ///     在 pivot 元素前面插入一个元素 value
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="pivot"></param>
    /// <param name="cacheValue"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<long> LInsertBeforeAsync<T>(string cacheKey, T pivot, T cacheValue, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var pivotBytes = _serializer.Serialize(pivot);
        var cacheValueBytes = _serializer.Serialize(cacheValue);
        
        return await _cache.ListInsertBeforeAsync(cacheKey, pivotBytes, cacheValueBytes);
    }

    /// <summary>
    ///     在 pivot 元素的后面插入一个元素 value
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="pivot"></param>
    /// <param name="cacheValue"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<long> LInsertAfterAsync<T>(string cacheKey, T pivot, T cacheValue, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var pivotBytes = _serializer.Serialize(pivot);
        var cacheValueBytes = _serializer.Serialize(cacheValue);
        
        return await _cache.ListInsertAfterAsync(cacheKey, pivotBytes, cacheValueBytes);
    }

    #endregion

    #region Set(数组操作)

    /// <summary>
    ///     向集合中添加一个元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <param name="expiration"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public long SAdd<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(cacheValues, nameof(cacheValues));

        var len = _cache.SetAdd(cacheKey, cacheValues.Select(x => (RedisValue)_serializer.Serialize(x)).ToArray());

        if (expiration.HasValue) _cache.KeyExpire(cacheKey, expiration.Value);

        return len;
    }

    /// <summary>
    ///     返回指定key集合中的元素数
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <returns></returns>
    public long SCard(string cacheKey)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        return _cache.SetLength(cacheKey);
    }

    /// <summary>
    ///     判断集合中是否存在元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool SIsMember<T>(string cacheKey, T cacheValue)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);

        return  _cache.SetContains(cacheKey, bytes);
    }

    /// <summary>
    ///     返回集合中的所有元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IEnumerable<T> SMembers<T>(string cacheKey)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            
        var bytes = _cache.SetMembers(cacheKey);
        foreach (var item in bytes) 
            yield return _serializer.Deserialize<T>(item);
    }
    
    /// <summary>
    ///     从集合中随机返回一个元素(不删除集合中的元素)
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="count"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IEnumerable<T> SRandMembers<T>(string cacheKey, int count = 1)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            
        var bytes = _cache.SetRandomMembers(cacheKey, count);
        foreach (var item in bytes) 
            yield return _serializer.Deserialize<T>(item);
    }

    /// <summary>
    ///     从集合中随机取出一个元素(会删除集合中的元素)
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T SPop<T>(string cacheKey)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _cache.SetPop(cacheKey);
        
        return _serializer.Deserialize<T>(bytes);
    }
    
    /// <summary>
    ///     从集合中移除一堆元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public long SRem<T>(string cacheKey, IList<T> cacheValues = null)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        long len;

        if (cacheValues != null && cacheValues.Any())
        {
            len = _cache.SetRemove(cacheKey, cacheValues.Select(x => (RedisValue)_serializer.Serialize(x)).ToArray());
        }
        else
        {
            var flag = _cache.KeyDelete(cacheKey);
            len = flag ? 1 : 0;
        }

        return len;
    }

    /// <summary>
    ///     向集合中添加一个元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <param name="expiration"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<long> SAddAsync<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(cacheValues, nameof(cacheValues));

        var len = await _cache.SetAddAsync(cacheKey, cacheValues.Select(x => (RedisValue)_serializer.Serialize(x)).ToArray());

        if (expiration.HasValue) await _cache.KeyExpireAsync(cacheKey, expiration.Value);

        return len;
    }

    /// <summary>
    ///     返回指定key集合中的元素数
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<long> SCardAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        
        return await _cache.SetLengthAsync(cacheKey);
    }

    /// <summary>
    ///     判断集合中是否存在元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValue"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<bool> SIsMemberAsync<T>(string cacheKey, T cacheValue, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);

        var flag = await _cache.SetContainsAsync(cacheKey, bytes);
        return flag;
    }

    /// <summary>
    ///     返回集合中的所有元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<IEnumerable<T>> SMembersAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var list = new List<T>();

        var vals = await _cache.SetMembersAsync(cacheKey);

        foreach (var item in vals) list.Add(_serializer.Deserialize<T>(item));

        return list;
    }

    /// <summary>
    ///     从集合中随机取出一个元素(会删除集合中的元素)
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<T> SPopAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = await _cache.SetPopAsync(cacheKey);

        return _serializer.Deserialize<T>(bytes);
    }

    /// <summary>
    ///     从集合中随机返回一个元素(不删除集合中的元素)
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="count"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<IEnumerable<T>> SRandMembersAsync<T>(string cacheKey, int count = 1, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var list = new List<T>();

        var bytes = await _cache.SetRandomMembersAsync(cacheKey, count);

        foreach (var item in bytes) list.Add(_serializer.Deserialize<T>(item));

        return list;
    }

    /// <summary>
    ///     从集合中移除一堆元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<long> SRemAsync<T>(string cacheKey, IList<T> cacheValues = null, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        long len;
        if (cacheValues != null && cacheValues.Any())
        {
            len = await _cache.SetRemoveAsync(cacheKey, cacheValues.Select(x => (RedisValue)_serializer.Serialize(x)).ToArray());
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

    /// <summary>
    ///     向有序集合添加一个或多个成员，或者更新已存在成员的分数
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public long ZAdd<T>(string cacheKey, Dictionary<T, double> cacheValues)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var values = cacheValues.Select(x => new SortedSetEntry(_serializer.Serialize(x.Key), x.Value)).ToArray();
        
        return _cache.SortedSetAdd(cacheKey, values);
    }

    /// <summary>
    ///     获取有序集合的成员数
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <returns></returns>
    public long ZCard(string cacheKey)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var len = _cache.SortedSetLength(cacheKey);
        return len;
    }

    /// <summary>
    ///     计算在有序集合中指定区间分数的成员数
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public long ZCount(string cacheKey, double min, double max)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        return _cache.SortedSetLength(cacheKey, min, max);
    }

    /// <summary>
    ///     在有序集合中计算指定字典区间内成员数量
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public long ZLexCount(string cacheKey, string min, string max)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        return _cache.SortedSetLengthByValue(cacheKey, min, max);
    }
    
    /// <summary>
    ///     对有序集合中的某个元素增加一个分值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="field"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public double ZIncrBy(string cacheKey, string field, double val = 1)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNullOrWhiteSpace(field, nameof(field));

        var value = _cache.SortedSetIncrement(cacheKey, field, val);
        return value;
    }

    /// <summary>
    ///     通过索引区间返回有序集合指定区间内的成员
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IEnumerable<T> ZRange<T>(string cacheKey, long start, long stop)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _cache.SortedSetRangeByRank(cacheKey, start, stop);
        foreach (var item in bytes) 
            yield return _serializer.Deserialize<T>(item);
    }

    /// <summary>
    ///     通过分数返回有序集合指定区间内的成员
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="count"></param>
    /// <param name="offset"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IEnumerable<T> ZRangeByScore<T>(string cacheKey, double min, double max, long count = -1, long offset = 0)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _cache.SortedSetRangeByScore(cacheKey, min, max, take: count, skip: offset);
        foreach (var item in bytes) 
            yield return _serializer.Deserialize<T>(item);
    }

    /// <summary>
    ///     返回有序集合中指定成员的索引
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public long? ZRank<T>(string cacheKey, T cacheValue)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);

        return _cache.SortedSetRank(cacheKey, bytes);
    }

    /// <summary>
    ///     返回有序集合中指定元素的分值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public double? ZScore<T>(string cacheKey, T cacheValue)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);

        return _cache.SortedSetScore(cacheKey, bytes);
    }
    
    /// <summary>
    ///     移除有序集合中的多个元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public long ZRem<T>(string cacheKey, IList<T> cacheValues)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        return _cache.SortedSetRemove(cacheKey, cacheValues.Select(x => (RedisValue)_serializer.Serialize(x)).ToArray());
    }

    /// <summary>
    ///     移除有序集合中给定的排名区间的所有成员
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="star"></param>
    /// <param name="stop"></param>
    /// <returns></returns>
    public long ZRemRangeByRank(string cacheKey, long star, long stop)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        
        return _cache.SortedSetRemoveRangeByRank(cacheKey, star, stop);
    }

    /// <summary>
    ///     移除有序集合中给定的分数区间的所有成员
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public long ZRemRangeByScore(string cacheKey, double min, double max)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        
        return _cache.SortedSetRemoveRangeByScore(cacheKey, min, max);
    }
    
    /// <summary>
    ///     添加一个元素到有序集合中,如果集合中存在 则会修改其对应的分值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<long> ZAddAsync<T>(string cacheKey, Dictionary<T, double> cacheValues, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var redisValues = cacheValues.Select(x => new SortedSetEntry(_serializer.Serialize(x), x.Value)).ToArray();

        return await _cache.SortedSetAddAsync(cacheKey, redisValues);
    }

    /// <summary>
    ///     获取有序集合的成员数
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<long> ZCardAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        
        return await _cache.SortedSetLengthAsync(cacheKey);
    }

    /// <summary>
    ///     计算在有序集合中指定区间分数的成员数
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<long> ZCountAsync(string cacheKey, double min, double max, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        return await _cache.SortedSetLengthAsync(cacheKey, min, max);
    }

    /// <summary>
    ///     在有序集合中计算指定字典区间内成员数量
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<long> ZLexCountAsync(string cacheKey, string min, string max, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        
        return await _cache.SortedSetLengthByValueAsync(cacheKey, min, max);
    }
    
    /// <summary>
    ///     有序集合中对指定成员的分数加上增量 increment
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="field"></param>
    /// <param name="val"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<double> ZIncrByAsync(string cacheKey, string field, double val = 1, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNullOrWhiteSpace(field, nameof(field));
        
        return await _cache.SortedSetIncrementAsync(cacheKey, field, val);
    }

    /// <summary>
    ///     通过索引区间返回有序集合指定区间内的成员
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<IEnumerable<T>> ZRangeAsync<T>(string cacheKey, long start, long stop, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var list = new List<T>();

        var bytes = await _cache.SortedSetRangeByRankAsync(cacheKey, start, stop);

        foreach (var item in bytes) list.Add(_serializer.Deserialize<T>(item));

        return list;
    }

    /// <summary>
    ///     通过分数返回有序集合指定区间内的成员
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    /// <param name="count"></param>
    /// <param name="offset"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<IEnumerable<T>> ZRangeByScoreAsync<T>(string cacheKey, double start, double stop, long count = 0, long offset = 0, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var list = new List<T>();

        var bytes = await _cache.SortedSetRangeByScoreAsync(cacheKey, start, stop, skip: offset, take: count);

        foreach (var item in bytes) list.Add(_serializer.Deserialize<T>(item));

        return list;
    }

    /// <summary>
    ///     返回有序集合中指定成员的索引
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValue"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<long?> ZRankAsync<T>(string cacheKey, T cacheValue, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);

        var index = await _cache.SortedSetRankAsync(cacheKey, bytes);

        return index;
    }

    /// <summary>
    ///     返回有序集中，成员的分数值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValue"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<double?> ZScoreAsync<T>(string cacheKey, T cacheValue, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = _serializer.Serialize(cacheValue);

        return await _cache.SortedSetScoreAsync(cacheKey, bytes);
    }
    
    /// <summary>
    ///     移除有序集合中的多个元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<long> ZRemAsync<T>(string cacheKey, IList<T> cacheValues, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

        var bytes = new List<RedisValue>();

        foreach (var item in cacheValues) bytes.Add(_serializer.Serialize(item));

        return await _cache.SortedSetRemoveAsync(cacheKey, bytes.ToArray());
    }

    /// <summary>
    ///     移除有序集合中给定的排名区间的所有成员
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="star"></param>
    /// <param name="stop"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<long> ZRemRangeByRankAsync(string cacheKey, long star, long stop, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        
        return await _cache.SortedSetRemoveRangeByRankAsync(cacheKey, star, stop);
    }

    /// <summary>
    ///     移除有序集合中给定的分数区间的所有成员
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<long> ZRemRangeByScoreAsync(string cacheKey, double min, double max, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        
        return await _cache.SortedSetRemoveRangeByScoreAsync(cacheKey, min, max);
    }

    #endregion

    #region Geo(经纬度操作)

    /// <summary>
    ///     添加地理位置的坐标
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public long GeoAdd(string cacheKey, IEnumerable<(double longitude, double latitude, string member)> values)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(values, nameof(values));

        var redisValues = values.Select(x => new GeoEntry(x.longitude, x.latitude, x.member)).ToArray();
        
        return _cache.GeoAdd(cacheKey, redisValues);
    }

    /// <summary>
    ///     添加地理位置的坐标
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="values"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<long> GeoAddAsync(string cacheKey, IEnumerable<(double longitude, double latitude, string member)> values, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(values, nameof(values));

        var redisValues = values.Select(x => new GeoEntry(x.longitude, x.latitude, x.member)).ToArray();
        
        return await _cache.GeoAddAsync(cacheKey, redisValues);
    }

    /// <summary>
    ///     计算两个位置之间的距离
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="member1"></param>
    /// <param name="member2"></param>
    /// <param name="unit"></param>
    /// <returns></returns>
    public double? GeoDist(string cacheKey, string member1, string member2, string unit = "m")
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNullOrWhiteSpace(member1, nameof(member1));
        Check.NotNullOrWhiteSpace(member2, nameof(member2));
        Check.NotNullOrWhiteSpace(unit, nameof(unit));
        
        return _cache.GeoDistance(cacheKey, member1, member2, GetGeoUnit(unit));
    }

    /// <summary>
    ///     计算两个位置之间的距离
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="member1"></param>
    /// <param name="member2"></param>
    /// <param name="unit"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<double?> GeoDistAsync(string cacheKey, string member1, string member2, string unit = "m", CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNullOrWhiteSpace(member1, nameof(member1));
        Check.NotNullOrWhiteSpace(member2, nameof(member2));
        Check.NotNullOrWhiteSpace(unit, nameof(unit));
        
        return await _cache.GeoDistanceAsync(cacheKey, member1, member2, GetGeoUnit(unit));
    }

    /// <summary>
    ///     返回一个或多个位置对象的 geo hash 值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="members"></param>
    /// <returns></returns>
    public string[] GeoHash(string cacheKey, IEnumerable<string> members)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(members, nameof(members));

        return _cache.GeoHash(cacheKey, members.Select(x => (RedisValue)x).ToArray());
    }

    /// <summary>
    ///     返回一个或多个位置对象的 geo hash 值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="members"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string[]> GeoHashAsync(string cacheKey, IEnumerable<string> members, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(members, nameof(members));

        return await _cache.GeoHashAsync(cacheKey, members.Select(x => (RedisValue)x).ToArray());
    }

    /// <summary>
    ///     获取地理位置的坐标
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="members"></param>
    /// <returns></returns>
    public IEnumerable<(decimal longitude, decimal latitude)?> GeoPos(string cacheKey, IEnumerable<string> members)
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

    /// <summary>
    ///     获取地理位置的坐标
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="members"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<(decimal longitude, decimal latitude)?>> GeoPosAsync(string cacheKey, IEnumerable<string> members, CancellationToken cancellationToken = default)
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

    /// <summary>
    ///     根据用户给定的经纬度坐标来获取指定范围内的地理位置集合
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="member"></param>
    /// <param name="radius"></param>
    /// <param name="unit"></param>
    /// <param name="count"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    public IEnumerable<(string member, double? distance)> GeoRadius(string cacheKey, string member, double radius, string unit = "m", int count = -1, ListSortDirection order = ListSortDirection.Ascending)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(member, nameof(member));

        var res = _cache.GeoRadius(cacheKey, member, radius, GetGeoUnit(unit), count, GetGeoOrder(order), GeoRadiusOptions.WithDistance);
        foreach (var item in res)
            yield return (item.Member, item.Distance);
    }

    /// <summary>
    ///     根据用户给定的经纬度坐标来获取指定范围内的地理位置集合
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="member"></param>
    /// <param name="radius"></param>
    /// <param name="unit"></param>
    /// <param name="count"></param>
    /// <param name="order"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<(string member, double? distance)>> GeoRadiusAsync(string cacheKey, string member, double radius, string unit = "m", int count = -1, ListSortDirection order = ListSortDirection.Ascending, CancellationToken cancellationToken = default)
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

    private Order GetGeoOrder(ListSortDirection order)
    {
        Order geoOrder;
        switch (order)
        {
            case ListSortDirection.Descending:
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

    /// <summary>
    ///     添加指定元素到 HyperLogLog 中
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="values"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool PfAdd<T>(string cacheKey, IEnumerable<T> values)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(values, nameof(values));

        var redisValues = values.Select(x => (RedisValue)_serializer.Serialize(x)).ToArray();
        
        return _cache.HyperLogLogAdd(cacheKey, redisValues);
    }

    /// <summary>
    ///     添加指定元素到 HyperLogLog 中
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="values"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<bool> PfAddAsync<T>(string cacheKey, IEnumerable<T> values, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
        Check.NotNull(values, nameof(values));

        var redisValues = values.Select(x => (RedisValue)_serializer.Serialize(x)).ToArray();
        
        return await _cache.HyperLogLogAddAsync(cacheKey, redisValues);
    }

    /// <summary>
    ///     返回给定 HyperLogLog 的基数估算值
    /// </summary>
    /// <param name="cacheKeys"></param>
    /// <returns></returns>
    public long PfCount(IEnumerable<string> cacheKeys)
    {
        Check.NotNull(cacheKeys, nameof(cacheKeys));
        
        return _cache.HyperLogLogLength(cacheKeys.Select(x => (RedisKey)x).ToArray());
    }

    /// <summary>
    ///     返回给定 HyperLogLog 的基数估算值
    /// </summary>
    /// <param name="cacheKeys"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<long> PfCountAsync(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default)
    {
        Check.NotNull(cacheKeys, nameof(cacheKeys));
        
        return await _cache.HyperLogLogLengthAsync(cacheKeys.Select(x => (RedisKey)x).ToArray());
    }

    /// <summary>
    ///     将多个 HyperLogLog 合并为一个 HyperLogLog
    /// </summary>
    /// <param name="destKey"></param>
    /// <param name="sourceKeys"></param>
    /// <returns></returns>
    public bool PfMerge(string destKey, IEnumerable<string> sourceKeys)
    {
        Check.NotNullOrWhiteSpace(destKey, nameof(destKey));
        Check.NotNull(sourceKeys, nameof(sourceKeys));

        _cache.HyperLogLogMerge(destKey, sourceKeys.Select(x => (RedisKey)x).ToArray());
        return true;
    }

    /// <summary>
    ///     将多个 HyperLogLog 合并为一个 HyperLogLog
    /// </summary>
    /// <param name="destKey"></param>
    /// <param name="sourceKeys"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> PfMergeAsync(string destKey, IEnumerable<string> sourceKeys, CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(destKey, nameof(destKey));
        Check.NotNull(sourceKeys, nameof(sourceKeys));

        await _cache.HyperLogLogMergeAsync(destKey, sourceKeys.Select(x => (RedisKey)x).ToArray());
        return true;
    }

    #endregion
}