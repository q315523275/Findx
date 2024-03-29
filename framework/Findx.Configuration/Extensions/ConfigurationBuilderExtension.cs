using Findx.Common;
using Microsoft.Extensions.Configuration;

namespace Findx.Configuration.Extensions
{
    public static class ConfigurationBuilderExtension
    {
        public static IConfigurationBuilder AddFindxConfig(this IConfigurationBuilder configurationBuilder,
            ConfigOptions options)
        {
            Check.NotNull(options, nameof(options));
            if (options.Enabled) configurationBuilder.Add(new ConfigSource(new ConfigClient(options)));
            return configurationBuilder;
        }

        public static IConfigurationBuilder AddFindxConfig(this IConfigurationBuilder configurationBuilder,
            IConfigClient client)
        {
            Check.NotNull(client, nameof(client));
            configurationBuilder.Add(new ConfigSource(client));
            return configurationBuilder;
        }

        public static IConfigurationBuilder AddFindxConfig(this IConfigurationBuilder configurationBuilder)
        {
            var config = configurationBuilder.Build();
            var options = config.GetSection("Findx:Config").Get<ConfigOptions>();
            if (options.Enabled) configurationBuilder.Add(new ConfigSource(new ConfigClient(options)));
            return configurationBuilder;
        }
    }
}