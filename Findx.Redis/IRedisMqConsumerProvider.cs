namespace Findx.Redis
{
    public interface IRedisMqConsumerProvider
    {
        IRedisMqConsumer Create(QueueConsumerConfiguration configuration);
        IRedisMqConsumer Create(QueueConsumerConfiguration configuration, RedisOptions options);
    }
}
