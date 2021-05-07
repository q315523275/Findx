using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Redis.StackExchangeRedis
{
    public class StackExchangeRedisMqConsumer : IRedisMqConsumer
    {

        private readonly IStackExchangeRedisDataBaseProvider _dataBaseProvider;

        private readonly RedisOptions _options;

        private readonly QueueConsumerConfiguration _configuration;

        private readonly ConcurrentBag<Func<string, string, Task>> _callbacks;

        private readonly ILogger<StackExchangeRedisMqConsumer> _logger;

        private IDatabase _cache;

        private IDatabase _cacheAsync;

        public StackExchangeRedisMqConsumer(IStackExchangeRedisDataBaseProvider dataBaseProvider, RedisOptions options, QueueConsumerConfiguration configuration, ILogger<StackExchangeRedisMqConsumer> logger)
        {
            Check.NotNull(configuration, nameof(configuration));

            _dataBaseProvider = dataBaseProvider;
            _options = options;
            _configuration = configuration;
            _logger = logger;
            _callbacks = new ConcurrentBag<Func<string, string, Task>>();
        }

        private void Connect()
        {
            if (_cache != null)
                return;

            _cache = _dataBaseProvider.GetConnection(_options).GetDatabase();
        }

        private async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (_cacheAsync != null)
                return;

            _cacheAsync = (await _dataBaseProvider.GetConnectionAsync(_options, cancellationToken)).GetDatabase();
        }


        public string RedisClientName => _options.Name;

        public void OnMessageReceived(Func<string, string, Task> callback)
        {
            _callbacks.Add(callback);
        }

        protected virtual async Task HandleIncomingMessage(StreamEntry streamEntry)
        {
            try
            {
                foreach (var callback in _callbacks)
                {
                    await callback(streamEntry.Id, streamEntry.Values[0].Value);
                }

                await _cacheAsync.StreamAcknowledgeAsync(_configuration.QueueName, _configuration.GroupName, streamEntry.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RedisMQ HandleIncomingMessage Error");
            }
        }

        public void StartConsuming()
        {
            Connect();

            // 消费组维护
            var groupList = _cache.StreamGroupInfo(_configuration.QueueName);
            if (!groupList.Any(it => it.Name == _configuration.GroupName))
            {
                _cache.StreamCreateConsumerGroup(_configuration.QueueName, _configuration.GroupName, position: "0-0");
            }

            Task.Factory.StartNew(async () =>
            {
                await ConnectAsync();

                while (true)
                {
                    var streamEntryList = await _cacheAsync.StreamReadGroupAsync(_configuration.QueueName, _configuration.GroupName, _configuration.ConsumerName, position: StreamPosition.Beginning);
                    if (streamEntryList.Length > 0)
                    {
                        foreach (var streamEntry in streamEntryList)
                        {
                            await HandleIncomingMessage(streamEntry);
                        }
                    }
                    else
                    {
                        await Task.Delay(500);
                    }
                }
            }, TaskCreationOptions.LongRunning);

        }
    }
}
