using Microsoft.Extensions.Configuration;

namespace Findx.Configuration.Extensions
{
    public static class ConfigurationBuilderExtension
    {
        public static IConfigurationBuilder AddFindxConfig(this IConfigurationBuilder configurationBuilder, ConfigOptions options)
        {
            Check.NotNull(options, nameof(options));
            configurationBuilder.Add(new ConfigSource(new ConfigClient(options)));
            return configurationBuilder;
        }
        
        public static IConfigurationBuilder AddFindxConfig(this IConfigurationBuilder configurationBuilder, IConfigClient client)
        {
            Check.NotNull(client, nameof(client));
            configurationBuilder.Add(new ConfigSource(client));
            return configurationBuilder;
        }

        public static IConfigurationBuilder AddFindxConfig(this IConfigurationBuilder configurationBuilder)
        {
            var config = configurationBuilder.Build();
            var options = new ConfigOptions();
            config.GetSection("Findx:Config").Bind(options);
            configurationBuilder.Add(new ConfigSource(new ConfigClient(options)));
            return configurationBuilder;
        }
    }
}