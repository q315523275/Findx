namespace Findx.Redis.StackExchangeRedis;

public class RedisClientProvider : IRedisClientProvider
{
    private readonly IConnectionProvider _connectionProvider;
    private readonly IRedisSerializer _serializer;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="connectionProvider"></param>
    /// <param name="serializer"></param>
    public RedisClientProvider(IConnectionProvider connectionProvider, IRedisSerializer serializer)
    {
        _connectionProvider = connectionProvider;
        _serializer = serializer;
    }

    public IRedisClient CreateClient(string connectionName = null)
    {
        return new RedisClient(_serializer, _connectionProvider.GetConnection(connectionName), connectionName ?? RedisConnections.DefaultConnectionName);
    }

    public IRedisClient CreateClient(IRedisSerializer redisSerializer, string connectionName = null)
    {
        return new RedisClient(redisSerializer, _connectionProvider.GetConnection(connectionName), connectionName ?? RedisConnections.DefaultConnectionName);
    }
}