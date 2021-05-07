using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Redis.StackExchangeRedis
{
    public class StackExchangeRedisMqPublisher : IRedisMqPublisher
    {
        private readonly IStackExchangeRedisDataBaseProvider _dataBaseProvider;

        private readonly RedisOptions _options;

        private readonly IRedisSerializer _serializer;

        private readonly SemaphoreSlim _connectionLock;

        public StackExchangeRedisMqPublisher(IStackExchangeRedisDataBaseProvider dataBaseProvider, RedisOptions options, IRedisSerializer serializer)
        {
            _dataBaseProvider = dataBaseProvider;
            _options = options;
            _serializer = serializer;
            _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        }

        private IDatabase _cache;

        private IDatabase _cacheAsync;

        public string RedisClientName => _options.Name;

        private void Connect()
        {
            if (_cache != null)
                return;

            _connectionLock.Wait();

            try
            {
                if (_cache != null)
                    return;

                _cache = _dataBaseProvider.GetConnection(_options).GetDatabase();
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (_cacheAsync != null)
                return;

            await _connectionLock.WaitAsync();

            try
            {
                if (_cacheAsync != null)
                    return;

                _cacheAsync = (await _dataBaseProvider.GetConnectionAsync(_options, cancellationToken)).GetDatabase();
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public void Publish<T>(T obj, string queue)
        {
            Check.NotNull(obj, nameof(obj));
            Check.NotNull(queue, nameof(queue));

            Connect();

            _cache.StreamAdd(queue, "text", _serializer.Serialize(obj));
        }

        public async Task PublishAsync<T>(T obj, string queue, CancellationToken cancellationToken = default)
        {
            Check.NotNull(obj, nameof(obj));
            Check.NotNull(queue, nameof(queue));

            await ConnectAsync();

            await _cacheAsync.StreamAddAsync(queue, "text", _serializer.Serialize(obj), messageId: "*");
        }
    }
}
