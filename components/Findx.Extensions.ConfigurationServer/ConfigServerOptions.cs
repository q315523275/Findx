using Microsoft.Extensions.Options;

namespace Findx.Extensions.ConfigurationServer;

/// <summary>
///     配置服务配置信息
/// </summary>
public class ConfigServerOptions : IOptions<ConfigServerOptions>
{
    /// <summary>
    ///     当前节点
    /// </summary>
    public string CurrentNode { set; get; }

    /// <summary>
    ///     集群节点
    /// </summary>
    public List<string> ClusterNodes { get; } = [];

    /// <summary>
    ///     this
    /// </summary>
    public ConfigServerOptions Value => this;
}