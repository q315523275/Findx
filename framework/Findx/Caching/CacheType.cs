namespace Findx.Caching;

/// <summary>
///     缓存服务类型
/// </summary>
public static class CacheType
{
    /// <summary>
    ///     内存
    /// </summary>
    public const string DefaultMemory = "memoryCache";

    /// <summary>
    ///     Redis
    /// </summary>
    public const string DefaultRedis = "redis.default";
}