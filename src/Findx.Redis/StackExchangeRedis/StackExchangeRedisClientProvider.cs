using Findx.Redis.StackExchangeRedis;

namespace Findx.Redis
{
    public class StackExchangeRedisClientProvider : IRedisClientProvider
    {
        private readonly IStackExchangeRedisConnectionProvider _connectionProvider;
        private readonly IRedisSerializer _serializer;

        public StackExchangeRedisClientProvider(IStackExchangeRedisConnectionProvider connectionProvider,
            IRedisSerializer serializer)
        {
            _connectionProvider = connectionProvider;
            _serializer = serializer;
        }

        public IRedisClient CreateClient(string connectionName = null)
        {
            return new StackExchangeRedisClient(_serializer, _connectionProvider.GetConnection(connectionName),
                connectionName ?? RedisConnections.DefaultConnectionName);
        }

        public IRedisClient CreateClient(IRedisSerializer redisSerializer, string connectionName = null)
        {
            return new StackExchangeRedisClient(redisSerializer, _connectionProvider.GetConnection(connectionName),
                connectionName ?? RedisConnections.DefaultConnectionName);
        }
    }
}