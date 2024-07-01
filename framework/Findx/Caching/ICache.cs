using Findx.DependencyInjection;

namespace Findx.Caching;

/// <summary>
///     缓存
/// </summary>
public partial interface ICache: IServiceNameAware
{
    /// <summary>
    ///     是否存在指定键的缓存
    /// </summary>
    /// <param name="key">缓存键</param>
    bool Exists(string key);
    
    /// <summary>
    ///     从缓存中获取数据
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    T Get<T>(string key);

    /// <summary>
    ///     当缓存数据不存在则添加，已存在不会添加，添加成功返回true
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">值</param>
    bool TryAdd<T>(string key, T value);
    
    /// <summary>
    ///     当缓存数据不存在则添加，已存在不会添加，添加成功返回true
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">值</param>
    /// <param name="absoluteExpiration">相对过期时间间隔</param>
    bool TryAdd<T>(string key, T value, TimeSpan absoluteExpiration);
    
    /// <summary>
    ///     当缓存数据不存在则添加，已存在不会添加，添加成功返回true
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">值</param>
    /// <param name="absoluteExpiration">绝对过期时间</param>
    bool TryAdd<T>(string key, T value, DateTime absoluteExpiration);

    /// <summary>
    ///     当缓存数据不存在则添加，已存在不会添加，添加成功返回true
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">值</param>
    /// <param name="slidingExpirationOptions">滑动过期配置</param>
    bool TryAdd<T>(string key, T value,SlidingExpirationOptions slidingExpirationOptions);

    /// <summary>
    ///     添加缓存。如果已存在缓存，将覆盖
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">值</param>
    void Add<T>(string key, T value);
    
    /// <summary>
    ///     添加缓存。如果已存在缓存，将覆盖
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">值</param>
    /// <param name="absoluteExpiration">相对过期时间</param>
    void Add<T>(string key, T value, TimeSpan absoluteExpiration);
    
    /// <summary>
    ///     添加缓存。如果已存在缓存，将覆盖
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">值</param>
    /// <param name="absoluteExpiration">绝对过期时间</param>
    void Add<T>(string key, T value, DateTime absoluteExpiration);
    
    /// <summary>
    ///     添加缓存。如果已存在缓存，将覆盖
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">值</param>
    /// <param name="slidingExpirationOptions">滑动过期配置</param>
    void Add<T>(string key, T value, SlidingExpirationOptions slidingExpirationOptions);

    /// <summary>
    ///     移除缓存
    /// </summary>
    /// <param name="key">缓存键</param>
    void Remove(string key);

    /// <summary>
    ///     通过缓存键前缀移除缓存
    /// </summary>
    /// <param name="prefix">缓存键前缀</param>
    void RemoveByPrefix(string prefix);

    /// <summary>
    ///     清空缓存
    /// </summary>
    void Clear();
}