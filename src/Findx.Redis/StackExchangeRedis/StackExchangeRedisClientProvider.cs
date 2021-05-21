using Microsoft.Extensions.Options;

namespace Findx.Redis.StackExchangeRedis
{
    public class StackExchangeRedisClientProvider : IRedisClientProvider
    {
        private readonly IStackExchangeRedisDataBaseProvider _redisDataBaseProvider;
        private readonly IOptionsMonitor<RedisOptions> _options;

        public StackExchangeRedisClientProvider(IStackExchangeRedisDataBaseProvider redisDataBaseProvider, IOptionsMonitor<RedisOptions> options)
        {
            _redisDataBaseProvider = redisDataBaseProvider;
            _options = options;
        }

        public IRedisClient CreateClient()
        {
            return new StackExchangeRedisClient(_redisDataBaseProvider, _options.CurrentValue, new RedisJsonSerializer());
        }

        public IRedisClient CreateClient(RedisOptions options)
        {
            return new StackExchangeRedisClient(_redisDataBaseProvider, options, new RedisJsonSerializer());
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
