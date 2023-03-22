using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Findx.Redis
{
    public interface IRedisClient
    {
        #region Public(公共操作)
        string Name { get; }


        /// <summary>
        /// 清除key
        /// </summary>
        void FlushDb();

        /// <summary>
        /// 清除key
        /// </summary>
        Task FlushDbAsync();

        /// <summary>
        /// 清除当前db的所有数据
        /// </summary>
        void Clear();

        /// <summary>
        /// 清除当前db的所有数据
        /// </summary>
        Task ClearAsync();

        /// <summary>
        /// 命令操作
        /// </summary>
        /// <param name="script"></param>
        /// <param name="cacheKey"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        object Eval(string script, string cacheKey, IEnumerable<object> args);

        /// <summary>
        /// 命令操作
        /// </summary>
        /// <param name="script"></param>
        /// <param name="cacheKey"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task<object> EvalAsync(string script, string cacheKey, IEnumerable<object> args);

        #endregion Public

        #region Keys(缓存Key操作)

        /// <summary>
        /// 查找当前命名前缀下共有多少个Key
        /// </summary>
        /// <returns></returns>
        int KeyCount();

        /// <summary>
        /// 查找键名
        /// </summary>
        /// <param name="pattern">匹配项</param>
        /// <returns>匹配上的所有键名</returns>
        IEnumerable<string> SearchKeys(string pattern);

        /// <summary>
        /// 查看缓存剩余时间
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        long Ttl(string key);

        /// <summary>
        /// 查看缓存剩余时间
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<long> TtlAsync(string key);

        /// <summary>
        /// 判断是否存在当前的Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Exists(string key);

        /// <summary>
        /// 判断是否存在当前的Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// 设置key的失效时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        bool Expire(string key, TimeSpan expiry);

        /// <summary>
        /// 设置key的失效时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        Task<bool> ExpireAsync(string key, TimeSpan expiry);

        /// <summary>
        /// 设置key的失效时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        bool Expire(string key, DateTime expiry);

        /// <summary>
        /// 设置key的失效时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        Task<bool> ExpireAsync(string key, DateTime expiry);

        /// <summary>
        /// 移除当前key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Remove(string key);

        /// <summary>
        /// 移除当前key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<bool> RemoveAsync(string key);

        /// <summary>
        /// 移除全部key
        /// </summary>
        /// <param name="keys"></param>
        void RemoveAll(IEnumerable<string> keys);

        /// <summary>
        /// 移除全部key
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        Task RemoveAllAsync(IEnumerable<string> keys);

        /// <summary>
        /// 计算当前prefix开头的key总数
        /// </summary>
        /// <param name="prefix">key前缀</param>
        /// <returns></returns>
        int CalcuteKeyCount(string prefix);

        /// <summary>
        /// 删除以当前prefix开头的所有key缓存
        /// </summary>
        /// <param name="prefix">key前缀</param>
        void DeleteKeyWithKeyPrefix(string prefix);

        /// <summary>
        /// 删除以当前prefix开头的所有key缓存
        /// </summary>
        /// <param name="prefix">key前缀</param>
        Task DeleteKeyWithKeyPrefixAsync(string prefix);
        #endregion Keys

        #region String(字符串类型数据操作)

        /// <summary>
        /// 设置string键值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">值</param>
        /// <returns>成功返回true</returns>
        bool StringSet<T>(string key, T value);

        /// <summary>
        /// 设置string键值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">值</param>
        /// <returns>成功返回true</returns>
        Task<bool> StringSetAsync<T>(string key, T value);

        /// <summary>
        /// 设置string键值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">值</param>
        /// <param name="expiresIn">过期间隔</param>
        /// <returns>成功返回true</returns>
        bool StringSet<T>(string key, T value, TimeSpan expiresIn);

        /// <summary>
        /// 设置string键值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">值</param>
        /// <param name="expiresIn">过期间隔</param>
        /// <returns>成功返回true</returns>
        Task<bool> StringSetAsync<T>(string key, T value, TimeSpan expiresIn);

        /// <summary>
        /// 设置string键值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">值</param>
        /// <param name="expiresAt">过期时间</param>
        /// <returns>成功返回true</returns>
        bool StringSet<T>(string key, T value, DateTimeOffset expiresAt);

        /// <summary>
        /// 设置string键值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">值</param>
        /// <param name="expiresAt">过期时间</param>
        /// <returns>成功返回true</returns>
        Task<bool> StringSetAsync<T>(string key, T value, DateTimeOffset expiresAt);

        /// <summary>
        /// 批量设置string键值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="items">键值列表</param>
        /// <returns>成功返回true</returns>
        bool StringSetAll<T>(IList<Tuple<string, T>> items);

        /// <summary>
        /// 批量设置string键值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="items">键值列表</param>
        /// <returns>成功返回true</returns>
        Task<bool> StringSetAllAsync<T>(IList<Tuple<string, T>> items);

        /// <summary>
        /// string获取值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <returns></returns>
        T StringGet<T>(string key);

        /// <summary>
        /// string获取值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        Task<T> StringGetAsync<T>(string key);

        /// <summary>
        /// 键值累加
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">增长数量</param>
        /// <returns>累加后的值</returns>
        long StringIncrement(string key, long value = 1);

        /// <summary>
        /// 键值累加
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">增长数量</param>
        /// <returns>累加后的值</returns>
        Task<long> StringIncrementAsync(string key, long value = 1);

        /// <summary>
        /// 键值累加
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">增长数量</param>
        /// <returns>累加后的值</returns>
        double StringIncrementDouble(string key, double value);

        /// <summary>
        /// 键值累加
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">增长数量</param>
        /// <returns>累加后的值</returns>
        Task<double> StringIncrementDoubleAsync(string key, double value);

        /// <summary>
        /// 键值递减
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">减少数量</param>
        /// <returns>递减后的值</returns>
        long StringDecrement(string key, long value = 1);

        /// <summary>
        /// 键值递减
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">减少数量</param>
        /// <returns>递减后的值</returns>
        Task<long> StringDecrementAsync(string key, long value = 1);

        /// <summary>
        /// 键值递减
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">减少数量</param>
        /// <returns>递减后的值</returns>
        double StringDecrementDouble(string key, double value);

        /// <summary>
        /// 键值递减
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">减少数量</param>
        /// <returns>递减后的值</returns>
        Task<double> StringDecrementDoubleAsync(string key, double value);

        #endregion StringSet

        #region Hash(哈希数据类型操作)

        /// <summary>
        /// 获取所有的Hash键
        /// </summary>
        /// <param name="key">key</param>
        /// <returns></returns>
        IEnumerable<string> HashKeys(string key);

        /// <summary>
        /// 获取hash键的个数
        /// </summary>
        /// <param name="key">key</param>
        /// <returns></returns>
        long HashLength(string key);

        /// <summary>
        /// 设置一个hash值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="hashField">hash的键值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        bool HashSet<T>(string key, string hashField, T value);

        /// <summary>
        /// 设置一个hash值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="hashField">hash的键值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        Task<bool> HashSetAsync<T>(string key, string hashField, T value);

        /// <summary>
        /// 批量设置hash值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="values">键值对</param>
        void HashSet<T>(string key, Dictionary<string, T> values);

        /// <summary>
        /// 批量设置hash值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="values">键值对</param>
        Task HashSetAsync<T>(string key, Dictionary<string, T> values);

        /// <summary>
        /// 获取一个hash值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <returns></returns>
        T HashGet<T>(string key, string hashField);

        /// <summary>
        /// 获取一个hash值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <returns></returns>
        Task<T> HashGetAsync<T>(string key, string hashField);

        /// <summary>
        /// 获取hash值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="hashFields">hash键组合</param>
        /// <returns></returns>
        Dictionary<string, T> HashGet<T>(string key, IEnumerable<string> hashFields);

        /// <summary>
        /// 获取hash值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="hashFields">hash键组合</param>
        /// <returns></returns>
        Task<Dictionary<string, T>> HashGetAsync<T>(string key, IEnumerable<string> hashFields);

        /// <summary>
        /// 获取全部hash值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">key</param>
        /// <returns></returns>
        Dictionary<string, T> HashGetAll<T>(string key);

        /// <summary>
        /// 获取全部hash值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">key</param>
        /// <returns></returns>
        Task<Dictionary<string, T>> HashGetAllAsync<T>(string key);

        /// <summary>
        /// 获取全部hash值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">key</param>
        /// <returns></returns>
        IEnumerable<T> HashValues<T>(string key);

        /// <summary>
        /// 获取全部hash值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">key</param>
        /// <returns></returns>
        Task<IEnumerable<T>> HashValuesAsync<T>(string key);

        /// <summary>
        /// 判断是否存在hash键
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <returns></returns>
        bool HashExists(string key, string hashField);

        /// <summary>
        /// 判断是否存在hash键
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <returns></returns>
        Task<bool> HashExistsAsync(string key, string hashField);

        /// <summary>
        /// 删除一个hash键
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <returns></returns>
        bool HashDelete(string key, string hashField);

        /// <summary>
        /// 删除一个hash键
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <returns></returns>
        Task<bool> HashDeleteAsync(string key, string hashField);

        /// <summary>
        /// 删除hash键
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashFields">hash键集合</param>
        /// <returns></returns>
        long HashDelete(string key, IEnumerable<string> hashFields);

        /// <summary>
        /// 删除hash键
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashFields">hash键集合</param>
        /// <returns></returns>
        Task<long> HashDeleteAsync(string key, IEnumerable<string> hashFields);

        /// <summary>
        /// hash递增
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <param name="value">递增值</param>
        /// <returns></returns>
        long HashIncrement(string key, string hashField, long value = 1);

        /// <summary>
        /// hash递增
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <param name="value">递增值</param>
        /// <returns></returns>
        Task<long> HashIncrementAsync(string key, string hashField, long value = 1);

        /// <summary>
        /// hash递减
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <param name="value">递减值</param>
        /// <returns></returns>
        long HashDecrement(string key, string hashField, long value = 1);

        /// <summary>
        /// hash递减
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <param name="value">递减值</param>
        /// <returns></returns>
        Task<long> HashDecrementAsync(string key, string hashField, long value = 1);

        /// <summary>
        /// hash递增
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <param name="value">递增值</param>
        /// <returns></returns>
        double HashIncrementDouble(string key, string hashField, double value);

        /// <summary>
        /// hash递增
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <param name="value">递增值</param>
        /// <returns></returns>
        Task<double> HashIncrementDoubleAsync(string key, string hashField, double value);

        /// <summary>
        /// hash递减
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <param name="value">递减值</param>
        /// <returns></returns>
        double HashDecrementDouble(string key, string hashField, double value);

        /// <summary>
        /// hash递减
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash键</param>
        /// <param name="value">递减值</param>
        /// <returns></returns>
        Task<double> HashDecrementDoubleAsync(string key, string hashField, double value);

        #endregion hash

        #region Lock(分布式锁操作)

        /// <summary>
        /// 获取一个锁
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">键名</param>
        /// <param name="value">值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>成功返回true</returns>
        bool LockTake<T>(string key, T value, TimeSpan expiry);

        /// <summary>
        /// 异步获取一个锁
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">键名</param>
        /// <param name="value">值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>成功返回true</returns>
        Task<bool> LockTakeAsync<T>(string key, T value, TimeSpan expiry);

        /// <summary>
        /// 释放一个锁
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">键名</param>
        /// <param name="value">值</param>
        /// <returns>成功返回true</returns>
        bool LockRelease<T>(string key, T value);

        /// <summary>
        /// 异步释放一个锁
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">键名</param>
        /// <param name="value">值</param>
        /// <returns>成功返回true</returns>
        Task<bool> LockReleaseAsync<T>(string key, T value);

        #endregion lock

        #region List(集合操作)
        T ListGetByIndex<T>(string cacheKey, long index);
        long ListLength(string cacheKey);
        T ListLeftPop<T>(string cacheKey);
        long ListLeftPush<T>(string cacheKey, T cacheValue);
        long ListLeftPush<T>(string cacheKey, IList<T> cacheValues);
        T ListRightPop<T>(string cacheKey);
        long ListRightPush<T>(string cacheKey, T cacheValue);
        long ListRightPush<T>(string cacheKey, IList<T> cacheValues);
        List<T> ListRange<T>(string cacheKey, long start, long stop);
        long ListRemove<T>(string cacheKey, long count, T cacheValue);
        bool ListSetByIndex<T>(string cacheKey, long index, T cacheValue);
        bool ListTrim(string cacheKey, long start, long stop);
        long ListInsertBefore<T>(string cacheKey, T pivot, T cacheValue);
        long ListInsertAfter<T>(string cacheKey, T pivot, T cacheValue);
        Task<T> ListGetByIndexAsync<T>(string cacheKey, long index);
        Task<long> ListLengthAsync(string cacheKey);
        Task<T> ListLeftPopAsync<T>(string cacheKey);
        Task<long> ListLeftPushAsync<T>(string cacheKey, T cacheValue);
        Task<long> ListLeftPushAsync<T>(string cacheKey, IList<T> cacheValues);
        Task<T> ListRightPopAsync<T>(string cacheKey);
        Task<long> ListRightPushAsync<T>(string cacheKey, T cacheValue);
        Task<long> ListRightPushAsync<T>(string cacheKey, IList<T> cacheValues);
        Task<List<T>> ListRangeAsync<T>(string cacheKey, long start, long stop);
        Task<long> ListRemoveAsync<T>(string cacheKey, long count, T cacheValue);
        Task<bool> ListSetByIndexAsync<T>(string cacheKey, long index, T cacheValue);
        Task<bool> ListTrimAsync(string cacheKey, long start, long stop);
        Task<long> ListInsertBeforeAsync<T>(string cacheKey, T pivot, T cacheValue);
        Task<long> ListInsertAfterAsync<T>(string cacheKey, T pivot, T cacheValue);
        #endregion

        #region Set(数组操作)
        long SetAdd<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null);
        long SetLength(string cacheKey);
        bool SetContains<T>(string cacheKey, T cacheValue);
        List<T> SetMembers<T>(string cacheKey);
        T SetPop<T>(string cacheKey);
        List<T> SetRandomMembers<T>(string cacheKey, int count = 1);
        long SetRemove<T>(string cacheKey, IList<T> cacheValues = null);
        Task<long> SetAddAsync<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null);
        Task<long> SetLengthAsync(string cacheKey);
        Task<bool> SetContainsAsync<T>(string cacheKey, T cacheValue);
        Task<List<T>> SetMembersAsync<T>(string cacheKey);
        Task<T> SetPopAsync<T>(string cacheKey);
        Task<List<T>> SetRandomMembersAsync<T>(string cacheKey, int count = 1);
        Task<long> SetRemoveAsync<T>(string cacheKey, IList<T> cacheValues = null);
        #endregion

        #region Sorted Set(有序数组)
        long SortedSetAdd<T>(string cacheKey, Dictionary<T, double> cacheValues);
        long SortedSetLength(string cacheKey);
        long SortedSetLengthByValue(string cacheKey, double min, double max);
        double SortedSetIncrement(string cacheKey, string field, double val = 1);
        long SortedSetLengthByValue(string cacheKey, string min, string max);
        List<T> SortedSetRangeByRank<T>(string cacheKey, long start, long stop);
        long? SortedSetRank<T>(string cacheKey, T cacheValue);
        long SortedSetRemove<T>(string cacheKey, IList<T> cacheValues);
        double? SortedSetScore<T>(string cacheKey, T cacheValue);
        Task<long> SortedSetAddAsync<T>(string cacheKey, Dictionary<T, double> cacheValues);
        Task<long> SortedSetLengthAsync(string cacheKey);
        Task<long> SortedSetLengthByValueAsync(string cacheKey, double min, double max);
        Task<double> SortedSetIncrementAsync(string cacheKey, string field, double val = 1);
        Task<long> SortedSetLengthByValueAsync(string cacheKey, string min, string max);
        Task<List<T>> SortedSetRangeByRankAsync<T>(string cacheKey, long start, long stop);
        Task<long?> SortedSetRankAsync<T>(string cacheKey, T cacheValue);
        Task<long> SortedSetRemoveAsync<T>(string cacheKey, IList<T> cacheValues);
        Task<double?> SortedSetScoreAsync<T>(string cacheKey, T cacheValue);
        #endregion

        #region Geo(经纬度操作)
        /// <summary>
        /// 添加地理位置的坐标
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        long GeoAdd(string cacheKey, IEnumerable<(double longitude, double latitude, string member)> values);
        /// <summary>
        /// 添加地理位置的坐标
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        Task<long> GeoAddAsync(string cacheKey, IEnumerable<(double longitude, double latitude, string member)> values);
        /// <summary>
        /// 计算两个位置之间的距离
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="member1"></param>
        /// <param name="member2"></param>
        /// <param name="unit">默认单位:米</param>
        /// <returns></returns>
        double? GeoDistance(string cacheKey, string member1, string member2, string unit = "m");
        /// <summary>
        /// 计算两个位置之间的距离
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="member1"></param>
        /// <param name="member2"></param>
        /// <param name="unit">默认单位:米</param>
        /// <returns></returns>
        Task<double?> GeoDistanceAsync(string cacheKey, string member1, string member2, string unit = "m");
        /// <summary>
        /// 返回一个或多个位置对象的 geohash 值
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        string[] GeoHash(string cacheKey, IEnumerable<string> members);
        /// <summary>
        /// 返回一个或多个位置对象的 geohash 值
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        Task<string[]> GeoHashAsync(string cacheKey, IEnumerable<string> members);
        /// <summary>
        /// 获取地理位置的坐标
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        List<(decimal longitude, decimal latitude)?> GeoPosition(string cacheKey, IEnumerable<string> members);
        /// <summary>
        /// 获取地理位置的坐标
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        Task<List<(decimal longitude, decimal latitude)?>> GeoPositionAsync(string cacheKey, IEnumerable<string> members);
        /// <summary>
        /// 根据用户给定的经纬度坐标来获取指定范围内的地理位置集合
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="member"></param>
        /// <param name="radius">范围值</param>
        /// <param name="unit">默认单位:米</param>
        /// <param name="count">取数,-1全部</param>
        /// <param name="order">排序方式</param>
        /// <returns></returns>
        List<(string member, double? distance)> GeoRadius(string cacheKey, string member, double radius, string unit = "m", int count = -1, string order = "asc");
        /// <summary>
        /// 根据用户给定的经纬度坐标来获取指定范围内的地理位置集合
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="member"></param>
        /// <param name="radius">范围值</param>
        /// <param name="unit">默认单位:米</param>
        /// <param name="count">取数,-1全部</param>
        /// <param name="order">排序方式</param>
        /// <returns></returns>
        Task<List<(string member, double? distance)>> GeoRadiusAsync(string cacheKey, string member, double radius, string unit = "m", int count = -1, string order = "asc");
        #endregion

        #region HyperLogLog
        /// <summary>
        /// 添加指定元素到 HyperLogLog 中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        bool HyperLogLogAdd<T>(string cacheKey, IEnumerable<T> values);
        /// <summary>
        /// 添加指定元素到 HyperLogLog 中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        Task<bool> HyperLogLogAddAsync<T>(string cacheKey, IEnumerable<T> values);
        /// <summary>
        /// 返回给定 HyperLogLog 的基数估算值
        /// </summary>
        /// <param name="cacheKeys"></param>
        /// <returns></returns>
        long HyperLogLogLength(IEnumerable<string> cacheKeys);
        /// <summary>
        /// 返回给定 HyperLogLog 的基数估算值
        /// </summary>
        /// <param name="cacheKeys"></param>
        /// <returns></returns>
        Task<long> HyperLogLogLengthAsync(IEnumerable<string> cacheKeys);
        /// <summary>
        /// 将多个 HyperLogLog 合并为一个 HyperLogLog
        /// </summary>
        /// <param name="destKey"></param>
        /// <param name="sourceKeys"></param>
        /// <returns></returns>
        bool HyperLogLogMerge(string destKey, IEnumerable<string> sourceKeys);
        /// <summary>
        /// 将多个 HyperLogLog 合并为一个 HyperLogLog
        /// </summary>
        /// <param name="destKey"></param>
        /// <param name="sourceKeys"></param>
        /// <returns></returns>
        Task<bool> HyperLogLogMergeAsync(string destKey, IEnumerable<string> sourceKeys);
        #endregion
    }
}
