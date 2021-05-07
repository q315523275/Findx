using Microsoft.Extensions.Options;

namespace Findx.Redis.StackExchangeRedis
{
    public class StackExchangeRedisMqPublisherProvider : IRedisMqPublisherProvider
    {
        private readonly IStackExchangeRedisDataBaseProvider _redisDataBaseProvider;
        private readonly IOptionsMonitor<RedisOptions> _options;

        public StackExchangeRedisMqPublisherProvider(IStackExchangeRedisDataBaseProvider redisDataBaseProvider, IOptionsMonitor<RedisOptions> options)
        {
            _redisDataBaseProvider = redisDataBaseProvider;
            _options = options;
        }

        public IRedisMqPublisher Create()
        {
            return new StackExchangeRedisMqPublisher(_redisDataBaseProvider, _options.CurrentValue, new RedisJsonSerializer());
        }

        public IRedisMqPublisher Create(RedisOptions options)
        {
            return new StackExchangeRedisMqPublisher(_redisDataBaseProvider, options, new RedisJsonSerializer());
        }

        public IRedisMqPublisher Create(IRedisSerializer redisSerializer)
        {
            return new StackExchangeRedisMqPublisher(_redisDataBaseProvider, _options.CurrentValue, redisSerializer);
        }

        public IRedisMqPublisher Create(RedisOptions options, IRedisSerializer redisSerializer)
        {
            return new StackExchangeRedisMqPublisher(_redisDataBaseProvider, options, redisSerializer);
        }
    }
}
