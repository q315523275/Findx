namespace Findx.Redis
{
    public interface IRedisClientProvider
    {
        IRedisClient CreateClient();
        IRedisClient CreateClient(RedisOptions options);
        IRedisClient CreateClient(IRedisSerializer redisSerializer);
        IRedisClient CreateClient(RedisOptions options, IRedisSerializer redisSerializer);
    }
}
