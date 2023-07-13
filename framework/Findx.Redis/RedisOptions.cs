using Microsoft.Extensions.Options;

namespace Findx.Redis
{
    public class RedisOptions: IOptions<RedisOptions>
    {
        public RedisConnections Connections { get; set; } = new();
        
        
        public bool Enabled { get; set; } = true;
        
        public RedisOptions Value => this;
    }
}