namespace Findx.Redis
{
    public class FindxRedisOptions
    {
        public FindxRedisOptions()
        {
            Connections = new RedisConnections();
        }

        public RedisConnections Connections { get; }
    }
}