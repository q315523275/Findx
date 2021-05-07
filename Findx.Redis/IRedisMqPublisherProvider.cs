namespace Findx.Redis
{
    public interface IRedisMqPublisherProvider
    {
        IRedisMqPublisher Create();
        IRedisMqPublisher Create(RedisOptions options);
        IRedisMqPublisher Create(IRedisSerializer redisSerializer);
        IRedisMqPublisher Create(RedisOptions options, IRedisSerializer redisSerializer);
    }
}
