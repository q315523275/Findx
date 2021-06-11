using Microsoft.Extensions.Options;

namespace Findx.Redis
{
    public class RedisOptions : IOptions<RedisOptions>
    {
        public RedisOptions Value => this;
        public string Configuration { get; set; }
        public string Name { get; set; } = "redis.default";

        public int StreamEntriesCount { get; set; } = 1;
    }
}
