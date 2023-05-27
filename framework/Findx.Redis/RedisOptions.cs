namespace Findx.Redis
{
    public class RedisOptions
    {
        public RedisOptions()
        {
            Connections = new RedisConnections();
        }

        public RedisConnections Connections { get; }
    }
}