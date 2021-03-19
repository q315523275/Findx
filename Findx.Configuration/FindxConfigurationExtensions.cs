using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Findx.Configuration
{
    public static class FindxConfigurationExtensions
    {
        public static IConfigurationBuilder AddFindx(this IConfigurationBuilder configurationBuilder, FindxConfigurationOptions options)
        {
            Check.NotNull(options, nameof(options));
            configurationBuilder.Add(new FindxConfigurationSource(options));
            return configurationBuilder;
        }

        public static IHostBuilder ConfigureFindxConfiguration(this IHostBuilder hostBuilder, FindxConfigurationOptions options)
        {
            hostBuilder.ConfigureHostConfiguration(builder =>
            {
                builder.AddFindx(options);
            });
            return hostBuilder;
        }
        public static IHostBuilder ConfigureFindxConfiguration(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureHostConfiguration(builder =>
            {
                FindxConfigurationOptions options = new FindxConfigurationOptions();
                var configuration = builder.Build();
                configuration.Bind(options);
                builder.AddFindx(options);
            });
            return hostBuilder;
        }
    }
}
