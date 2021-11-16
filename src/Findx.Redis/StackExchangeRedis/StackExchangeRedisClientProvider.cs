using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Threading;

namespace Findx.Redis.StackExchangeRedis
{
    public class StackExchangeRedisClientProvider : IRedisClientProvider
    {
        private readonly IStackExchangeRedisDataBaseProvider _redisDataBaseProvider;
        private readonly IOptionsMonitor<RedisOptions> _options;
        private readonly IRedisSerializer _serializer;

        private readonly ConcurrentDictionary<string, StackExchangeRedisClient> _clients;
        private readonly SemaphoreSlim _connectionLock;

        public StackExchangeRedisClientProvider(IStackExchangeRedisDataBaseProvider redisDataBaseProvider, IOptionsMonitor<RedisOptions> options, IRedisSerializer serializer)
        {
            _redisDataBaseProvider = redisDataBaseProvider;
            _options = options;
            _serializer = serializer;

            _clients = new ConcurrentDictionary<string, StackExchangeRedisClient>();
            _connectionLock = new SemaphoreSlim(1, 1);
        }

        public IRedisClient CreateClient()
        {
            if (_clients.TryGetValue(_options.CurrentValue.Name, out var _client))
                return _client;

            _connectionLock.Wait();

            try
            {
                if (_clients.TryGetValue(_options.CurrentValue.Name, out _client))
                    return _client;

                _client = new StackExchangeRedisClient(_redisDataBaseProvider, _options.CurrentValue, _serializer);

                _clients[_options.CurrentValue.Name] = _client;

                return _client;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public IRedisClient CreateClient(RedisOptions options)
        {
            if (_clients.TryGetValue(options.Name, out var _client))
                return _client;

            _connectionLock.Wait();

            try
            {
                if (_clients.TryGetValue(options.Name, out _client))
                    return _client;

                _client = new StackExchangeRedisClient(_redisDataBaseProvider, options, _serializer);

                _clients[options.Name] = _client;

                return _client;
            }
            finally
            {
                _connectionLock.Release();
            }
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
