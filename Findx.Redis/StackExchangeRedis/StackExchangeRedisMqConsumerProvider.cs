using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Redis.StackExchangeRedis
{
    public class StackExchangeRedisMqConsumerProvider : IRedisMqConsumerProvider
    {
        private readonly IStackExchangeRedisDataBaseProvider _redisDataBaseProvider;
        private readonly IOptionsMonitor<RedisOptions> _options;
        private readonly ILogger<StackExchangeRedisMqConsumer> _logger;

        public StackExchangeRedisMqConsumerProvider(IStackExchangeRedisDataBaseProvider redisDataBaseProvider, IOptionsMonitor<RedisOptions> options, ILogger<StackExchangeRedisMqConsumer> logger)
        {
            _redisDataBaseProvider = redisDataBaseProvider;
            _options = options;
            _logger = logger;
        }

        public IRedisMqConsumer Create(QueueConsumerConfiguration configuration)
        {
            return new StackExchangeRedisMqConsumer(_redisDataBaseProvider, _options.CurrentValue, configuration, _logger);
        }

        public IRedisMqConsumer Create(QueueConsumerConfiguration configuration, RedisOptions options)
        {
            return new StackExchangeRedisMqConsumer(_redisDataBaseProvider, options, configuration, _logger);
        }
    }
}
