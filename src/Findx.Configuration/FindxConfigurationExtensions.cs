using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Findx.Configuration
{
    /// <summary>
    /// Findx配置扩展
    /// </summary>
    public static class FindxConfigurationExtensions
    {
        /// <summary>
        /// 添加Findx配置中心控制
        /// </summary>
        /// <param name="configurationBuilder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddFindx(this IConfigurationBuilder configurationBuilder, FindxConfigurationOptions options)
        {
            Check.NotNull(options, nameof(options));
            configurationBuilder.Add(new FindxConfigurationSource(options));
            return configurationBuilder;
        }
        
        /// <summary>
        /// 配置Findx配置中心控制
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IHostBuilder ConfigureFindxConfiguration(this IHostBuilder hostBuilder, FindxConfigurationOptions options)
        {
            hostBuilder.ConfigureHostConfiguration(builder =>
            {
                builder.AddFindx(options);
            });
            return hostBuilder;
        }
        
        /// <summary>
        /// 配置Findx配置中心控制
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IHostBuilder ConfigureFindxConfiguration(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureHostConfiguration(builder =>
            {
                var options = new FindxConfigurationOptions();
                var configuration = builder.Build();
                configuration.Bind(options);
                builder.AddFindx(options);
            });
            return hostBuilder;
        }
    }
}
