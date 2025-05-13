using Findx.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Findx.Configuration.Extensions;

/// <summary>
///     主机构建扩展
/// </summary>
public static class HostBuilderExtension
{
    /// <summary>
    ///     使用配置中心
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    public static IHostBuilder UseFindxConfig(this IHostBuilder builder, IConfigClient client)
    {
        builder.ConfigureAppConfiguration((_, conf) => { conf.AddFindxConfig(client); });
        return builder;
    }

    /// <summary>
    ///     使用配置中心
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder UseFindxConfig(this IHostBuilder builder, ConfigOptions options)
    {
        builder.ConfigureAppConfiguration((_, conf) => { conf.AddFindxConfig(options); });
        return builder;
    }

    /// <summary>
    ///     使用配置中心
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHostBuilder UseFindxConfig(this IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, conf) =>
        {
            var options = conf.Build().GetSection("Findx:Config").Get<ConfigOptions>();
            conf.AddFindxConfig(options);
        });
        return builder;
    }
    
    // /// <summary>
    // ///     使用配置中心
    // /// </summary>
    // /// <param name="builder"></param>
    // /// <returns></returns>
    // public static IHostApplicationBuilder UseFindxConfig(this IHostApplicationBuilder builder)
    // {
    //     var configuration = builder.Services.GetConfiguration();
    //     
    //     var options = configuration.GetSection("Findx:Config").Get<ConfigOptions>();
    //
    //     var configurationBuilder = builder.Services.GetSingletonInstanceOrNull<IConfigurationBuilder>();
    //     
    //     configurationBuilder.AddFindxConfig(options);
    //     
    //     return builder;
    // }
}