using Microsoft.Extensions.Configuration;

namespace Findx.Configuration
{
    internal class ConfigSource : IConfigurationSource
    {
        private readonly IConfigClient _client;
        
        public ConfigSource(IConfigClient client)
        {
            _client = client;
        }
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ConfigProvider(_client);
        }
    }
}
