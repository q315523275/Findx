using System.Threading.Tasks;

namespace Findx.Caching;

/// <summary>
///     缓存 - 异步
/// </summary>
public partial interface ICache
{
    /// <summary>
    ///     是否存在指定键的缓存
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="token"></param>
    Task<bool> ExistsAsync(string key, CancellationToken token = default);

    /// <summary>
    ///     从缓存中获取数据
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="cancellationToken"></param>
    Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    ///     当缓存数据不存在则添加，已存在不会添加，添加成功返回true
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken"></param>
    Task<bool> TryAddAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    /// <summary>
    ///     当缓存数据不存在则添加，已存在不会添加，添加成功返回true
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">值</param>
    /// <param name="absoluteExpiration">相对过期时间间隔</param>
    /// <param name="cancellationToken"></param>
    Task<bool> TryAddAsync<T>(string key, T value, TimeSpan absoluteExpiration, CancellationToken cancellationToken = default);

    /// <summary>
    ///     当缓存数据不存在则添加，已存在不会添加，添加成功返回true
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">值</param>
    /// <param name="absoluteExpiration">绝对过期时间</param>
    /// <param name="cancellationToken"></param>
    Task<bool> TryAddAsync<T>(string key, T value, DateTime absoluteExpiration, CancellationToken cancellationToken = default);

    /// <summary>
    ///     当缓存数据不存在则添加，已存在不会添加，添加成功返回true
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">值</param>
    /// <param name="slidingExpirationOptions">滑动过期配置</param>
    /// <param name="cancellationToken"></param>
    Task<bool> TryAddAsync<T>(string key, T value,SlidingExpirationOptions slidingExpirationOptions, CancellationToken cancellationToken = default);

    /// <summary>
    ///     添加缓存。如果已存在缓存，将覆盖
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken"></param>
    Task AddAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    /// <summary>
    ///     添加缓存。如果已存在缓存，将覆盖
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">值</param>
    /// <param name="absoluteExpiration">相对过期时间</param>
    /// <param name="cancellationToken"></param>
    Task AddAsync<T>(string key, T value, TimeSpan absoluteExpiration, CancellationToken cancellationToken = default);

    /// <summary>
    ///     添加缓存。如果已存在缓存，将覆盖
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">值</param>
    /// <param name="absoluteExpiration">绝对过期时间</param>
    /// <param name="cancellationToken"></param>
    Task AddAsync<T>(string key, T value, DateTime absoluteExpiration, CancellationToken cancellationToken = default);

    /// <summary>
    ///     添加缓存。如果已存在缓存，将覆盖
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">值</param>
    /// <param name="slidingExpirationOptions">滑动过期配置</param>
    /// <param name="cancellationToken"></param>
    Task AddAsync<T>(string key, T value, SlidingExpirationOptions slidingExpirationOptions, CancellationToken cancellationToken = default);

    /// <summary>
    ///     移除缓存
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="token"></param>
    Task RemoveAsync(string key, CancellationToken token = default);

    /// <summary>
    ///     通过缓存键前缀移除缓存
    /// </summary>
    /// <param name="prefix">缓存键前缀</param>
    /// <param name="token"></param>
    Task RemoveByPrefixAsync(string prefix, CancellationToken token = default);

    /// <summary>
    ///     清空缓存
    /// </summary>
    Task ClearAsync(CancellationToken token = default);
}