using Microsoft.Extensions.Options;

namespace Findx.Redis
{
    public class RedisCacheOptions : IOptions<RedisCacheOptions>
    {
        public RedisCacheOptions Value => this;
        public string Configuration { get; set; }
        public string InstanceName { get; set; }
        public string Prefix { get; set; }
        public override string ToString()
        {
            return $"{Configuration}{InstanceName}{Prefix}";
        }
    }
}
