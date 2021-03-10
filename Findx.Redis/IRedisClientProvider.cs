namespace Findx.Redis
{
    public interface IRedisClientProvider
    {
        IRedisClient CreateClient();
        IRedisClient CreateClient(RedisCacheOptions options);
        IRedisClient CreateClient(IRedisSerializer redisSerializer);
        IRedisClient CreateClient(RedisCacheOptions options, IRedisSerializer redisSerializer);
    }
}
