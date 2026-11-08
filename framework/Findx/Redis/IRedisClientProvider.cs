namespace Findx.Redis;

/// <summary>
///     Redis客户端提供器
/// </summary>
public interface IRedisClientProvider
{
    /// <summary>
    ///     创建redis客户端服务
    /// </summary>
    /// <remarks>
    ///     返回的客户端不独占底层连接：底层连接由本提供器的实现统一持有、复用与释放
    ///     （随提供器自身的生命周期关闭），调用方无需、也不应对返回的 <see cref="IRedisClient"/> 执行 Dispose。
    /// </remarks>
    /// <param name="connectionName"></param>
    /// <returns></returns>
    IRedisClient CreateClient(string connectionName = null);

    /// <summary>
    ///     创建redis客户端服务
    /// </summary>
    /// <remarks>
    ///     返回的客户端不独占底层连接：底层连接由本提供器的实现统一持有、复用与释放
    ///     （随提供器自身的生命周期关闭），调用方无需、也不应对返回的 <see cref="IRedisClient"/> 执行 Dispose。
    /// </remarks>
    /// <param name="redisSerializer"></param>
    /// <param name="connectionName"></param>
    /// <returns></returns>
    IRedisClient CreateClient(IRedisSerializer redisSerializer, string connectionName = null);
}