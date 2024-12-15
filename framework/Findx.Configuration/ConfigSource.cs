using Microsoft.Extensions.Configuration;

namespace Findx.Configuration;

/// <summary>
///     配置来源
/// </summary>
internal class ConfigSource : IConfigurationSource
{
    private readonly IConfigClient _client;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="client"></param>
    public ConfigSource(IConfigClient client)
    {
        _client = client;
    }

    /// <summary>
    ///     配置提供器构建
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new ConfigProvider(_client);
    }
}