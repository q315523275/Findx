using Microsoft.Extensions.Configuration;

namespace Findx.Setting
{
    public class ConfigurationSettingProvider : ISettingProvider
    {
        private readonly IConfiguration configuration;

        public ConfigurationSettingProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public T GetObject<T>(string key) where T : new()
        {
            return configuration.GetSection("key").Get<T>();
        }

        public T GetValue<T>(string key)
        {
            return configuration.GetValue<T>(key);
        }
    }
}

