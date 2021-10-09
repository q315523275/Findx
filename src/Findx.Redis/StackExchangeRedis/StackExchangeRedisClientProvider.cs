using Microsoft.Extensions.Options;

namespace Findx.Redis.StackExchangeRedis
{
    public class StackExchangeRedisClientProvider : IRedisClientProvider
    {
        private readonly IStackExchangeRedisDataBaseProvider _redisDataBaseProvider;
        private readonly IOptionsMonitor<RedisOptions> _options;
        private readonly IRedisSerializer _serializer;

        public StackExchangeRedisClientProvider(IStackExchangeRedisDataBaseProvider redisDataBaseProvider, IOptionsMonitor<RedisOptions> options)
        {
            _redisDataBaseProvider = redisDataBaseProvider;
            _options = options;
            _serializer = new RedisJsonSerializer();
        }

        public IRedisClient CreateClient()
        {
            return new StackExchangeRedisClient(_redisDataBaseProvider, _options.CurrentValue, _serializer);
        }

        public IRedisClient CreateClient(RedisOptions options)
        {
            return new StackExchangeRedisClient(_redisDataBaseProvider, options, _serializer);
        }

        public IRedisClient CreateClient(IRedisSerializer redisSerializer)
        {
            return new StackExchangeRedisClient(_redisDataBaseProvider, _options.CurrentValue, redisSerializer);
        }

        public IRedisClient CreateClient(RedisOptions options, IRedisSerializer redisSerializer)
        {
            return new StackExchangeRedisClient(_redisDataBaseProvider, options, redisSerializer);
        }
    }
}
