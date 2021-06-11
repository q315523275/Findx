using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Redis
{
    public interface IRedisStreamManager
    {
        Task CreateStreamWithConsumerGroupAsync(string stream, string consumerGroup);
        Task PublishAsync(string stream, NameValueEntry[] message);
        IAsyncEnumerable<RedisStream[]> PollStreamsLatestMessagesAsync(string[] streams, string consumerGroup, TimeSpan pollDelay, CancellationToken token);
        IAsyncEnumerable<RedisStream[]> PollStreamsPendingMessagesAsync(string[] streams, string consumerGroup, TimeSpan pollDelay, CancellationToken token);
        Task Ack(string stream, string consumerGroup, string messageId);
    }
}
