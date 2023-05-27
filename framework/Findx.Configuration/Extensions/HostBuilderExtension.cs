using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Findx.Configuration.Extensions
{
    public static class HostBuilderExtension
    {
        public static IHostBuilder UseFindxConfig(this IHostBuilder builder, IConfigClient client)
        {
            builder.ConfigureAppConfiguration((_, conf) => { conf.AddFindxConfig(client); });
            return builder;
        }

        public static IHostBuilder UseFindxConfig(this IHostBuilder builder, ConfigOptions options)
        {
            builder.ConfigureAppConfiguration((_, conf) => { conf.AddFindxConfig(options); });
            return builder;
        }

        public static IHostBuilder UseFindxConfig(this IHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, conf) =>
            {
                var config = conf.Build();
                conf.AddFindxConfig(config.GetSection("Findx:Config").Get<ConfigOptions>());
            });
            return builder;
        }
    }
}