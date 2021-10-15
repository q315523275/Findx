using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Redis.StackExchangeRedis
{
    internal class StackExchangeRedisStreamManager : IRedisStreamManager
    {
        private readonly IStackExchangeRedisDataBaseProvider _redisDataBaseProvider;
        private readonly RedisOptions _options;
        private readonly ILogger<StackExchangeRedisStreamManager> _logger;


        private ConnectionMultiplexer _redis;

        public StackExchangeRedisStreamManager(IStackExchangeRedisDataBaseProvider redisDataBaseProvider, IOptions<RedisOptions> options, ILogger<StackExchangeRedisStreamManager> logger)
        {
            _redisDataBaseProvider = redisDataBaseProvider;
            _options = options.Value;
            _logger = logger;
        }

        private async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            _redis = await _redisDataBaseProvider.GetConnectionAsync(_options, cancellationToken);
        }

        public async Task CreateStreamWithConsumerGroupAsync(string stream, string consumerGroup)
        {
            await ConnectAsync();

            var database = _redis.GetDatabase();
            var streamExist = await database.KeyTypeAsync(stream);
            if (streamExist == RedisType.None)
            {
                await database.StreamCreateConsumerGroupAsync(stream, consumerGroup, StreamPosition.NewMessages);
            }
            else
            {
                var groupInfo = await database.StreamGroupInfoAsync(stream);
                if (groupInfo.Any(g => g.Name == consumerGroup))
                    return;
                await database.StreamCreateConsumerGroupAsync(stream, consumerGroup, StreamPosition.NewMessages);
            }
        }

        public async Task PublishAsync(string stream, NameValueEntry[] message)
        {
            await ConnectAsync();

            await _redis.GetDatabase().StreamAddAsync(stream, message);
        }

        public async IAsyncEnumerable<RedisStream[]> PollStreamsLatestMessagesAsync(string[] streams, string consumerGroup, TimeSpan pollDelay, [EnumeratorCancellation] CancellationToken token)
        {
            var positions = streams.Select(stream => new StreamPosition(stream, StreamPosition.NewMessages));

            while (true)
            {
                var result = await TryReadConsumerGroup(consumerGroup, positions.ToArray(), token).ConfigureAwait(false);

                yield return result.streams;

                token.WaitHandle.WaitOne(pollDelay);
            }
        }

        public async IAsyncEnumerable<RedisStream[]> PollStreamsPendingMessagesAsync(string[] streams, string consumerGroup, TimeSpan pollDelay, [EnumeratorCancellation] CancellationToken token)
        {
            var positions = streams.Select(stream => new StreamPosition(stream, StreamPosition.Beginning));

            while (true)
            {
                token.ThrowIfCancellationRequested();

                var result = await TryReadConsumerGroup(consumerGroup, positions.ToArray(), token).ConfigureAwait(false);

                yield return result.streams;

                if (result.canRead && result.streams.All(s => s.Entries.Length < _options.StreamEntriesCount))
                    break;

                token.WaitHandle.WaitOne(pollDelay);
            }
        }

        public async Task Ack(string stream, string consumerGroup, string messageId)
        {
            await ConnectAsync();

            await _redis.GetDatabase().StreamAcknowledgeAsync(stream, consumerGroup, messageId).ConfigureAwait(false);
        }

        private async Task<(bool canRead, RedisStream[] streams)> TryReadConsumerGroup(string consumerGroup, StreamPosition[] positions, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                var createdPositions = new List<StreamPosition>();

                await ConnectAsync();

                var database = _redis.GetDatabase();

                await foreach (var position in database.TryCreateConsumerGroup(positions, consumerGroup, _logger).WithCancellation(token))
                    createdPositions.Add(position);

                if (!createdPositions.Any())
                    return (false, Array.Empty<RedisStream>());

                var readSet = database.StreamReadGroupAsync(createdPositions.ToArray(), consumerGroup, consumerGroup, _options.StreamEntriesCount);

                return (true, await readSet.ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Redis error when trying read consumer group {consumerGroup}");
                return (false, Array.Empty<RedisStream>());
            }
        }
    }
}
