using System.ComponentModel;
using System.Threading.Tasks;
using Findx.Common;

namespace Findx.Redis;

/// <summary>
///     redis客户端服务
/// </summary>
public interface IRedisClient
{
    #region Keys(缓存Key操作)

    /// <summary>
    ///     客户端名称
    /// </summary>
    string Name { get; }
    
    /// <summary>
    ///     命令操作
    /// </summary>
    /// <param name="script"></param>
    /// <param name="cacheKey"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    object Eval(string script, string cacheKey, IEnumerable<object> args);

    /// <summary>
    ///     命令操作
    /// </summary>
    /// <param name="script"></param>
    /// <param name="cacheKey"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    Task<object> EvalAsync(string script, string cacheKey, IEnumerable<object> args);

    /// <summary>
    ///     查找键名
    /// </summary>
    /// <param name="pattern">匹配项</param>
    /// <param name="count">数量</param>
    /// <returns>匹配上的所有键名</returns>
    IEnumerable<string> SearchKeys(string pattern, int? count);

    /// <summary>
    ///     查看缓存剩余时间
    /// </summary>
    /// <param name="key"></param>
    /// <returns>秒</returns>
    long Ttl(string key);

    /// <summary>
    ///     查看缓存剩余时间
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>秒</returns>
    Task<long> TtlAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    ///     判断是否存在当前的Key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    bool Exists(string key);

    /// <summary>
    ///     判断是否存在当前的Key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    ///     设置key的失效时间
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    bool Expire(string key, TimeSpan expiry);

    /// <summary>
    ///     设置key的失效时间
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiry"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> ExpireAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default);

    /// <summary>
    ///     移除当前key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    bool Remove(string key);

    /// <summary>
    ///     移除当前key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    ///     移除全部key
    /// </summary>
    /// <param name="keys"></param>
    long RemoveAll(IEnumerable<string> keys);

    /// <summary>
    ///     移除全部key
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long> RemoveAllAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    #endregion Keys

    #region String(字符串类型数据操作)

    /// <summary>
    ///     设置string键值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="value">值</param>
    /// <param name="expiration">过期间隔</param>
    /// <param name="when">key不存在时设置</param>
    /// <returns>成功返回true</returns>
    bool StringSet(string key, string value, TimeSpan? expiration = null, When when = When.Always);

    /// <summary>
    ///     设置string键值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="value">值</param>
    /// <param name="expiration">过期间隔</param>
    /// <param name="when">key不存在时设置</param>
    /// <param name="cancellationToken"></param>
    /// <returns>成功返回true</returns>
    Task<bool> StringSetAsync(string key, string value, TimeSpan? expiration, When when = When.Always, CancellationToken cancellationToken = default);

    /// <summary>
    ///     string获取值
    /// </summary>
    /// <param name="key">key</param>
    /// <returns></returns>
    string StringGet(string key);

    /// <summary>
    ///     string获取值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="cancellationToken"></param>
    Task<string> StringGetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    ///     键值累加
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">增长数量</param>
    /// <returns>累加后的值</returns>
    long IncrBy(string key, long value = 1);

    /// <summary>
    ///     键值累加
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">增长数量</param>
    /// <param name="cancellationToken"></param>
    /// <returns>累加后的值</returns>
    Task<long> IncrByAsync(string key, long value = 1, CancellationToken cancellationToken = default);

    /// <summary>
    ///     键值累加
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">增长数量</param>
    /// <returns>累加后的值</returns>
    double IncrByFloat(string key, double value);

    /// <summary>
    ///     键值累加
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">增长数量</param>
    /// <param name="cancellationToken"></param>
    /// <returns>累加后的值</returns>
    Task<double> IncrByFloatAsync(string key, double value, CancellationToken cancellationToken = default);

    /// <summary>
    ///     键值递减
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">减少数量</param>
    /// <returns>递减后的值</returns>
    long DecrBy(string key, long value = 1);

    /// <summary>
    ///     键值递减
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">减少数量</param>
    /// <param name="cancellationToken"></param>
    /// <returns>递减后的值</returns>
    Task<long> DecrByAsync(string key, long value = 1, CancellationToken cancellationToken = default);

    /// <summary>
    ///     键值递减
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">减少数量</param>
    /// <returns>递减后的值</returns>
    double DecrByFloat(string key, double value);

    /// <summary>
    ///     键值递减
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">减少数量</param>
    /// <param name="cancellationToken"></param>
    /// <returns>递减后的值</returns>
    Task<double> DecrByFloatAsync(string key, double value, CancellationToken cancellationToken = default);

    #endregion StringSet

    #region Hash(哈希数据类型操作)

    /// <summary>
    ///     设置一个hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash的键值</param>
    /// <param name="value">值</param>
    /// <param name="when"></param>
    /// <returns></returns>
    bool HSet(string key, string hashField, string value, When when = When.Always);

    /// <summary>
    ///     设置一个hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash的键值</param>
    /// <param name="value">值</param>
    /// <param name="when"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> HSetAsync(string key, string hashField, string value, When when = When.Always, CancellationToken cancellationToken = default);

    /// <summary>
    ///     批量设置hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="values">键值对</param>
    /// <param name="expiration">过期时间</param>
    // ReSharper disable once InconsistentNaming
    bool HMSet(string key, Dictionary<string, string> values, TimeSpan? expiration = null);

    /// <summary>
    ///     批量设置hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="values">键值对</param>
    /// <param name="expiration">过期时间</param>
    /// <param name="cancellationToken"></param>
    // ReSharper disable once InconsistentNaming
    Task<bool> HMSetAsync(string key, Dictionary<string, string> values, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     获取一个hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <returns></returns>
    string HGet(string key, string hashField);

    /// <summary>
    ///     获取一个hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> HGetAsync(string key, string hashField, CancellationToken cancellationToken = default);

    /// <summary>
    ///     获取hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashFields">hash键组合</param>
    /// <returns></returns>
    // ReSharper disable once InconsistentNaming
    Dictionary<string, string> HMGet(string key, IList<string> hashFields);

    /// <summary>
    ///     获取hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashFields">hash键组合</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    // ReSharper disable once InconsistentNaming
    Task<Dictionary<string, string>> HMGetAsync(string key, IList<string> hashFields, CancellationToken cancellationToken = default);

    /// <summary>
    ///     获取全部hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <returns></returns>
    Dictionary<string, string> HGetAll(string key);

    /// <summary>
    ///     获取全部hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Dictionary<string, string>> HGetAllAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    ///     获取全部hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <returns></returns>
    IEnumerable<string> HVals(string key);

    /// <summary>
    ///     获取全部hash值
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<string>> HValsAsync(string key, CancellationToken cancellationToken = default);

    
    /// <summary>
    ///     获取所有的Hash键
    /// </summary>
    /// <param name="key">key</param>
    /// <returns></returns>
    IEnumerable<string> HKeys(string key);

    /// <summary>
    ///     获取hash键的个数
    /// </summary>
    /// <param name="key">key</param>
    /// <returns></returns>
    long HLen(string key);
    
    /// <summary>
    ///     判断是否存在hash键
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <returns></returns>
    bool HExists(string key, string hashField);

    /// <summary>
    ///     判断是否存在hash键
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> HExistsAsync(string key, string hashField, CancellationToken cancellationToken = default);

    /// <summary>
    ///     删除一个hash键
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <returns></returns>
    bool HDel(string key, string hashField);

    /// <summary>
    ///     删除一个hash键
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> HDelAsync(string key, string hashField, CancellationToken cancellationToken = default);

    /// <summary>
    ///     删除hash键
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashFields">hash键集合</param>
    /// <returns></returns>
    long HDel(string key, IEnumerable<string> hashFields);

    /// <summary>
    ///     删除hash键
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashFields">hash键集合</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long> HDelAsync(string key, IEnumerable<string> hashFields, CancellationToken cancellationToken = default);

    /// <summary>
    ///     hash递增
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="value">递增值</param>
    /// <returns></returns>
    long HIncrBy(string key, string hashField, long value = 1);

    /// <summary>
    ///     hash递增
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="value">递增值</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long> HIncrByAsync(string key, string hashField, long value = 1, CancellationToken cancellationToken = default);

    /// <summary>
    ///     hash递减
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="value">递减值</param>
    /// <returns></returns>
    long HDecr(string key, string hashField, long value = 1);

    /// <summary>
    ///     hash递减
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="value">递减值</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long> HDecrAsync(string key, string hashField, long value = 1, CancellationToken cancellationToken = default);

    /// <summary>
    ///     hash递增
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="value">递增值</param>
    /// <returns></returns>
    double HIncrByFloat(string key, string hashField, double value);

    /// <summary>
    ///     hash递增
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="value">递增值</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<double> HIncrByFloatAsync(string key, string hashField, double value, CancellationToken cancellationToken = default);

    /// <summary>
    ///     hash递减
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="value">递减值</param>
    /// <returns></returns>
    double HDecrByFloat(string key, string hashField, double value);

    /// <summary>
    ///     hash递减
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="hashField">hash键</param>
    /// <param name="value">递减值</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<double> HDecrByFloatAsync(string key, string hashField, double value, CancellationToken cancellationToken = default);

    #endregion hash

    #region Lock(分布式锁操作)

    /// <summary>
    ///     获取一个锁
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">值</param>
    /// <param name="expiry">过期时间</param>
    /// <returns>成功返回true</returns>
    bool Lock(string key, string value, TimeSpan expiry);

    /// <summary>
    ///     异步获取一个锁
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">值</param>
    /// <param name="expiry">过期时间</param>
    /// <param name="cancellationToken"></param>
    /// <returns>成功返回true</returns>
    Task<bool> LockAsync(string key, string value, TimeSpan expiry, CancellationToken cancellationToken = default);

    /// <summary>
    ///     释放一个锁
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">值</param>
    /// <returns>成功返回true</returns>
    bool LockRelease(string key, string value);

    /// <summary>
    ///     异步释放一个锁
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken"></param>
    /// <returns>成功返回true</returns>
    Task<bool> LockReleaseAsync(string key, string value, CancellationToken cancellationToken = default);

    #endregion lock

    #region List(集合操作)

    /// <summary>
    ///     获取列表中某个位置的元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="index"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T LIndex<T>(string cacheKey, long index);

    /// <summary>
    ///     列表中的元素个数
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <returns></returns>
    long LLen(string cacheKey);
    
    /// <summary>
    ///     将一个值插入到列表头部
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>列表中的元素个数</returns>
    long LPush<T>(string cacheKey, T cacheValue);

    /// <summary>
    ///     将多个值插入到列表头部
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>列表中的元素个数</returns>
    long LPush<T>(string cacheKey, IList<T> cacheValues);

    /// <summary>
    ///     移出并获取列表的第一个元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T LPop<T>(string cacheKey);
    
    /// <summary>
    ///     获取列表指定范围内的元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="start"></param>
    /// <param name="stop">-1表示列表的最后一个元素,-2表示列表的倒数第二个元素</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IEnumerable<T> LRange<T>(string cacheKey, long start, long stop);

    /// <summary>
    ///     根据参数 COUNT 的值，移除列表中与参数 VALUE 相等的元素。
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="count">等于0删除所有,大于0从表头开始向表尾搜索删除最多count个与value相等的项,小于0从表尾开始向表头搜索删除最多count个与value相等的项</param>
    /// <param name="cacheValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>删除的数量</returns>
    long LRem<T>(string cacheKey, long count, T cacheValue);

    /// <summary>
    ///     通过索引设置列表元素的值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="index"></param>
    /// <param name="cacheValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    bool LSet<T>(string cacheKey, long index, T cacheValue);

    /// <summary>
    ///     对一个列表进行修剪(trim)
    /// </summary>
    /// <remarks>让列表只保留指定区间内的元素，不在指定区间之内的元素都将被删除</remarks>
    /// <param name="cacheKey"></param>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    /// <returns></returns>
    bool LTrim(string cacheKey, long start, long stop);

    /// <summary>
    ///     在 pivot 元素前面插入一个元素 value
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="pivot"></param>
    /// <param name="cacheValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>插入成功返回列表总长度,插入失败返回-1</returns>
    long LInsertBefore<T>(string cacheKey, T pivot, T cacheValue);

    /// <summary>
    ///     在 pivot 元素的后面插入一个元素 value
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="pivot"></param>
    /// <param name="cacheValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>插入成功返回的列表总长度,插入失败返回-1</returns>
    long LInsertAfter<T>(string cacheKey, T pivot, T cacheValue);

    /// <summary>
    ///     从列表的右侧插入值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    long RPush<T>(string cacheKey, T cacheValue);

    /// <summary>
    ///     从列表的右侧插入多个值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>列表中的元素个数</returns>
    long RPush<T>(string cacheKey, IList<T> cacheValues);
    
    /// <summary>
    ///     移出并获取列表的最后一个元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T RPop<T>(string cacheKey);
    
    /// <summary>
    ///     获取列表中某个位置的元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="index"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<T> LIndexAsync<T>(string cacheKey, long index, CancellationToken cancellationToken = default);

    /// <summary>
    ///     列表中的元素个数
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long> LLenAsync(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    ///     从列表的左侧插入值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValue"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>列表中的元素个数</returns>
    Task<long> LPushAsync<T>(string cacheKey, T cacheValue, CancellationToken cancellationToken = default);

    /// <summary>
    ///     从列表的左侧插入一堆值 从数组的第一个开始
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>列表中的元素个数</returns>
    Task<long> LPushAsync<T>(string cacheKey, IList<T> cacheValues, CancellationToken cancellationToken = default);

    /// <summary>
    ///     从列表的左侧取出一个值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<T> LPopAsync<T>(string cacheKey, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     取出列表中的值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="start"></param>
    /// <param name="stop">-1表示列表的最后一个元素,-2表示列表的倒数第二个元素</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<IEnumerable<T>> LRangeAsync<T>(string cacheKey, long start, long stop, CancellationToken cancellationToken = default);

    /// <summary>
    ///     删除列表中的一个元素,可设置要删除的数量,返回删除的数量
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="count">等于0删除所有,大于0从左到右删除最多count个与value相等的项,小于0从右到左删除最多count个与value相等的项</param>
    /// <param name="cacheValue"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>删除的数量</returns>
    Task<long> LRemAsync<T>(string cacheKey, long count, T cacheValue, CancellationToken cancellationToken = default);

    /// <summary>
    ///     设置列表中某个位置的元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="index"></param>
    /// <param name="cacheValue"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<bool> LSetAsync<T>(string cacheKey, long index, T cacheValue, CancellationToken cancellationToken = default);

    /// <summary>
    ///     按指定范围裁剪列表
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> LTrimAsync(string cacheKey, long start, long stop, CancellationToken cancellationToken = default);

    /// <summary>
    ///     在 pivot 元素前面插入一个元素 value
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="pivot"></param>
    /// <param name="cacheValue"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>插入成功返回列表总长度,插入失败返回-1</returns>
    Task<long> LInsertBeforeAsync<T>(string cacheKey, T pivot, T cacheValue, CancellationToken cancellationToken = default);

    /// <summary>
    ///     在 pivot 元素的后面插入一个元素 value
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="pivot"></param>
    /// <param name="cacheValue"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>插入成功返回的列表总长度,插入失败返回-1</returns>
    Task<long> LInsertAfterAsync<T>(string cacheKey, T pivot, T cacheValue, CancellationToken cancellationToken = default);

    /// <summary>
    ///     从列表的右侧插入值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValue"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<long> RPushAsync<T>(string cacheKey, T cacheValue, CancellationToken cancellationToken = default);

    /// <summary>
    ///     从列表的右侧插入一堆值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>列表中的元素个数</returns>
    Task<long> RPushAsync<T>(string cacheKey, IList<T> cacheValues, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     从列表的右侧取出一个值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<T> RPopAsync<T>(string cacheKey, CancellationToken cancellationToken = default);
    
    #endregion

    #region Set(数组操作)

    /// <summary>
    ///     向集合中添加多个元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <param name="expiration"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>返回是否添加成功</returns>
    long SAdd<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null);

    /// <summary>
    ///     返回指定key集合中的元素数
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <returns>返回是否添加成功</returns>
    long SCard(string cacheKey);

    /// <summary>
    ///     判断集合中是否存在元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    bool SIsMember<T>(string cacheKey, T cacheValue);

    /// <summary>
    ///     返回集合中的所有元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IEnumerable<T> SMembers<T>(string cacheKey);
    
    /// <summary>
    ///     从集合中随机返回一个元素(不删除集合中的元素)
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="count"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IEnumerable<T> SRandMembers<T>(string cacheKey, int count = 1);

    /// <summary>
    ///     从集合中随机取出一个元素(会删除集合中的元素)
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T SPop<T>(string cacheKey);
    
    /// <summary>
    ///     从集合中移除一堆元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>移除元素的个数</returns>
    long SRem<T>(string cacheKey, IList<T> cacheValues = null);

    /// <summary>
    ///     向集合中添加一个元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <param name="expiration"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>返回是否添加成功</returns>
    Task<long> SAddAsync<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     返回指定key集合中的元素数
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>返回是否添加成功</returns>
    Task<long> SCardAsync(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    ///     判断集合中是否存在元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValue"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<bool> SIsMemberAsync<T>(string cacheKey, T cacheValue, CancellationToken cancellationToken = default);

    /// <summary>
    ///     返回集合中的所有元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<IEnumerable<T>> SMembersAsync<T>(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    ///     从集合中随机取出一个元素(会删除集合中的元素)
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<T> SPopAsync<T>(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    ///     从集合中随机返回一个元素(不删除集合中的元素)
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="count"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<IEnumerable<T>> SRandMembersAsync<T>(string cacheKey, int count = 1, CancellationToken cancellationToken = default);

    /// <summary>
    ///     从集合中移除一堆元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cacheValues"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>移除元素的个数</returns>
    Task<long> SRemAsync<T>(string cacheKey, IList<T> cacheValues = null, CancellationToken cancellationToken = default);

    #endregion

    #region Sorted Set(有序数组)

    /// <summary>
    ///     向有序集合添加一个或多个成员，或者更新已存在成员的分数
    /// </summary>
    /// <param name="cacheKey">集合key</param>
    /// <param name="memberValues">成员集合信息</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>是否添加成功</returns>
    long ZAdd<T>(string cacheKey, Dictionary<T, double> memberValues);

    /// <summary>
    ///     获取有序集合的成员数
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <returns></returns>
    long ZCard(string cacheKey);

    /// <summary>
    ///     计算在有序集合中指定区间分数的成员数
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    long ZCount(string cacheKey, double min, double max);

    /// <summary>
    ///     在有序集合中计算指定字典区间内成员数量
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    long ZLexCount(string cacheKey, string min, string max);
    
    /// <summary>
    ///     对有序集合中的某个元素增加一个分值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="field"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    double ZIncrBy(string cacheKey, string field, double val = 1);
    
    /// <summary>
    ///     通过索引区间返回有序集合指定区间内的成员
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="start">以0表示有序集第一个成员,以1表示有序集第二个成员</param>
    /// <param name="stop">-1表示最后一个成员,-2表示倒数第二个成员</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IEnumerable<T> ZRange<T>(string cacheKey, long start, long stop);

    /// <summary>
    ///     通过分数返回有序集合指定区间内的成员
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="count">数量</param>
    /// <param name="offset">偏移值</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IEnumerable<T> ZRangeByScore<T>(string cacheKey, double min, double max, long count = -1, long offset = 0);
    
    /// <summary>
    ///     返回有序集合中指定成员的索引
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="memberValue">成员</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    long? ZRank<T>(string cacheKey, T memberValue);
    
    /// <summary>
    ///     返回有序集合中指定元素的分值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="memberValue">成员</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    double? ZScore<T>(string cacheKey, T memberValue);
    
    /// <summary>
    ///     移除有序集合中的多个元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="memberValues">成员集合</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>移除的个数</returns>
    long ZRem<T>(string cacheKey, IList<T> memberValues);
    
    /// <summary>
    ///     移除有序集合中给定的排名区间的所有成员
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="star"></param>
    /// <param name="stop"></param>
    /// <returns></returns>
    long ZRemRangeByRank(string cacheKey, long star, long stop);
    
    /// <summary>
    ///     移除有序集合中给定的分数区间的所有成员
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    long ZRemRangeByScore(string cacheKey, double min, double max);
    
    /// <summary>
    ///     添加一个元素到有序集合中,如果集合中存在 则会修改其对应的分值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="memberValues"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>是否添加成功</returns>
    Task<long> ZAddAsync<T>(string cacheKey, Dictionary<T, double> memberValues, CancellationToken cancellationToken = default);

    /// <summary>
    ///     获取有序集合的成员数
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long> ZCardAsync(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    ///     计算在有序集合中指定区间分数的成员数
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long> ZCountAsync(string cacheKey, double min, double max, CancellationToken cancellationToken = default);

    /// <summary>
    ///     在有序集合中计算指定字典区间内成员数量
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long> ZLexCountAsync(string cacheKey, string min, string max, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     有序集合中对指定成员的分数加上增量 increment
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="field"></param>
    /// <param name="val"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<double> ZIncrByAsync(string cacheKey, string field, double val = 1, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     通过索引区间返回有序集合指定区间内的成员
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="start">以0表示有序集第一个成员,以1表示有序集第二个成员</param>
    /// <param name="stop">-1表示最后一个成员,-2表示倒数第二个成员</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<IEnumerable<T>> ZRangeAsync<T>(string cacheKey, long start, long stop, CancellationToken cancellationToken = default);

    /// <summary>
    ///     通过分数返回有序集合指定区间内的成员
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="count"></param>
    /// <param name="offset"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<IEnumerable<T>> ZRangeByScoreAsync<T>(string cacheKey, double min, double max, long count = 0, long offset = 0, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     返回有序集合中指定成员的索引
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="memberValue"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<long?> ZRankAsync<T>(string cacheKey, T memberValue, CancellationToken cancellationToken = default);

    /// <summary>
    ///     返回有序集中，成员的分数值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="memberValue"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<double?> ZScoreAsync<T>(string cacheKey, T memberValue, CancellationToken cancellationToken = default);

    /// <summary>
    ///     移除有序集合中的多个元素
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="memberValues">成员集合</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>移除的个数</returns>
    Task<long> ZRemAsync<T>(string cacheKey, IList<T> memberValues, CancellationToken cancellationToken = default);

    /// <summary>
    ///     移除有序集合中给定的排名区间的所有成员
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="star"></param>
    /// <param name="stop"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long> ZRemRangeByRankAsync(string cacheKey, long star, long stop, CancellationToken cancellationToken = default);

    /// <summary>
    ///     移除有序集合中给定的分数区间的所有成员
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long> ZRemRangeByScoreAsync(string cacheKey, double min, double max, CancellationToken cancellationToken = default);
    
    #endregion

    #region Geo(经纬度操作)

    /// <summary>
    ///     添加地理位置的坐标
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    long GeoAdd(string cacheKey, IEnumerable<(double longitude, double latitude, string member)> values);

    /// <summary>
    ///     添加地理位置的坐标
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="values"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long> GeoAddAsync(string cacheKey, IEnumerable<(double longitude, double latitude, string member)> values, CancellationToken cancellationToken = default);

    /// <summary>
    ///     计算两个位置之间的距离
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="member1"></param>
    /// <param name="member2"></param>
    /// <param name="unit">默认单位:米</param>
    /// <returns></returns>
    double? GeoDist(string cacheKey, string member1, string member2, string unit = "m");

    /// <summary>
    ///     计算两个位置之间的距离
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="member1"></param>
    /// <param name="member2"></param>
    /// <param name="unit">默认单位:米</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<double?> GeoDistAsync(string cacheKey, string member1, string member2, string unit = "m", CancellationToken cancellationToken = default);

    /// <summary>
    ///     返回一个或多个位置对象的 geo hash 值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="members"></param>
    /// <returns></returns>
    string[] GeoHash(string cacheKey, IEnumerable<string> members);

    /// <summary>
    ///     返回一个或多个位置对象的 geo hash 值
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="members"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string[]> GeoHashAsync(string cacheKey, IEnumerable<string> members, CancellationToken cancellationToken = default);

    /// <summary>
    ///     获取地理位置的坐标
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="members"></param>
    /// <returns></returns>
    IEnumerable<(decimal longitude, decimal latitude)?> GeoPos(string cacheKey, IEnumerable<string> members);

    /// <summary>
    ///     获取地理位置的坐标
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="members"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<(decimal longitude, decimal latitude)?>> GeoPosAsync(string cacheKey, IEnumerable<string> members, CancellationToken cancellationToken = default);

    /// <summary>
    ///     根据用户给定的经纬度坐标来获取指定范围内的地理位置集合
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="member"></param>
    /// <param name="radius">范围值</param>
    /// <param name="unit">默认单位:米</param>
    /// <param name="count">取数,-1全部</param>
    /// <param name="order">排序方式</param>
    /// <returns></returns>
    IEnumerable<(string member, double? distance)> GeoRadius(string cacheKey, string member, double radius, string unit = "m"
        , int count = -1, ListSortDirection order = ListSortDirection.Ascending);

    /// <summary>
    ///     根据用户给定的经纬度坐标来获取指定范围内的地理位置集合
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="member"></param>
    /// <param name="radius">范围值</param>
    /// <param name="unit">默认单位:米</param>
    /// <param name="count">取数,-1全部</param>
    /// <param name="order">排序方式</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<(string member, double? distance)>> GeoRadiusAsync(string cacheKey, string member, double radius, string unit = "m"
        , int count = -1, ListSortDirection order = ListSortDirection.Ascending, CancellationToken cancellationToken = default);

    #endregion

    #region HyperLogLog

    /// <summary>
    ///     添加指定元素到 HyperLogLog 中
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cacheKey"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    bool PfAdd<T>(string cacheKey, IEnumerable<T> values);

    /// <summary>
    ///     添加指定元素到 HyperLogLog 中
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cacheKey"></param>
    /// <param name="values"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> PfAddAsync<T>(string cacheKey, IEnumerable<T> values, CancellationToken cancellationToken = default);

    /// <summary>
    ///     返回给定 HyperLogLog 的基数估算值
    /// </summary>
    /// <param name="cacheKeys"></param>
    /// <returns></returns>
    long PfCount(IEnumerable<string> cacheKeys);

    /// <summary>
    ///     返回给定 HyperLogLog 的基数估算值
    /// </summary>
    /// <param name="cacheKeys"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long> PfCountAsync(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default);

    /// <summary>
    ///     将多个 HyperLogLog 合并为一个 HyperLogLog
    /// </summary>
    /// <param name="destKey"></param>
    /// <param name="sourceKeys"></param>
    /// <returns></returns>
    bool PfMerge(string destKey, IEnumerable<string> sourceKeys);

    /// <summary>
    ///     将多个 HyperLogLog 合并为一个 HyperLogLog
    /// </summary>
    /// <param name="destKey"></param>
    /// <param name="sourceKeys"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> PfMergeAsync(string destKey, IEnumerable<string> sourceKeys, CancellationToken cancellationToken = default);

    #endregion
}