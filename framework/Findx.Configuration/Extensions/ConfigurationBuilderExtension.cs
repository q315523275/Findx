using System;
using Findx.Common;
using Microsoft.Extensions.Configuration;

namespace Findx.Configuration.Extensions;

/// <summary>
///     配置中心扩展
/// </summary>
public static class ConfigurationBuilderExtension
{
    /// <summary>
    ///     添加配置中心
    /// </summary>
    /// <param name="configurationBuilder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IConfigurationBuilder AddFindxConfig(this IConfigurationBuilder configurationBuilder, ConfigOptions options)
    {
        Check.NotNull(options, nameof(options));
        if (options.Enabled) configurationBuilder.Add(new ConfigSource(new ConfigClient(options)));
        return configurationBuilder;
    }
    
    /// <summary>
    ///     添加配置中心
    /// </summary>
    /// <param name="configurationBuilder"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IConfigurationBuilder AddFindxConfig(this IConfigurationBuilder configurationBuilder, Action<ConfigOptions> action)
    {
        var options = new ConfigOptions();
        action.Invoke(options);
        return AddFindxConfig(configurationBuilder, options);
    }

    /// <summary>
    ///     添加配置中心
    /// </summary>
    /// <param name="configurationBuilder"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    public static IConfigurationBuilder AddFindxConfig(this IConfigurationBuilder configurationBuilder, IConfigClient client)
    {
        Check.NotNull(client, nameof(client));
        configurationBuilder.Add(new ConfigSource(client));
        return configurationBuilder;
    }

    /// <summary>
    ///     添加配置中心
    /// </summary>
    /// <param name="configurationBuilder"></param>
    /// <returns></returns>
    public static IConfigurationBuilder AddFindxConfig(this IConfigurationBuilder configurationBuilder)
    {
        var config = configurationBuilder.Build();
        var options = config.GetSection("Findx:Config").Get<ConfigOptions>();
        if (options.Enabled) configurationBuilder.Add(new ConfigSource(new ConfigClient(options)));
        return configurationBuilder;
    }
}