using Microsoft.Extensions.Options;

namespace Findx.Component.DistributedConfigurationCenter;

/// <summary>
///     配置服务配置信息
/// </summary>
public class ConfigServiceOptions : IOptions<ConfigServiceOptions>
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
    public ConfigServiceOptions Value => this;
}