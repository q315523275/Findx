using Microsoft.Extensions.Options;

namespace Findx.Redis.StackExchangeRedis
{
    public class StackExchangeRedisClientProvider : IRedisClientProvider
    {
        private readonly IConnectionPool _pool;
        private readonly RedisCacheOptions _options;

        public StackExchangeRedisClientProvider(IConnectionPool pool, IOptions<RedisCacheOptions> options)
        {
            _pool = pool;
            _options = options.Value;
        }

        public IRedisClient CreateClient()
        {
            return new StackExchangeRedisClient(_pool, _options, new RedisJsonSerializer());
        }

        public IRedisClient CreateClient(RedisCacheOptions options)
        {
            return new StackExchangeRedisClient(_pool, options, new RedisJsonSerializer());
        }

        public IRedisClient CreateClient(IRedisSerializer redisSerializer)
        {
            return new StackExchangeRedisClient(_pool, _options, redisSerializer);
        }

        public IRedisClient CreateClient(RedisCacheOptions options, IRedisSerializer redisSerializer)
        {
            return new StackExchangeRedisClient(_pool, options, redisSerializer);
        }
    }
}
