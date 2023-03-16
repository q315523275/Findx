using Microsoft.Extensions.Options;

namespace Findx.Module.ConfigService;

/// <summary>
/// 配置服务配置信息
/// </summary>
public class ConfigServiceOptions: IOptions<ConfigServiceOptions>
{
    /// <summary>
    /// this
    /// </summary>
    public ConfigServiceOptions Value => this;

    /// <summary>
    /// 当前节点
    /// </summary>
    public string CurrentNode { set; get; }

    /// <summary>
    /// 集群节点
    /// </summary>
    public List<string> ClusterNodes { get; } = new();
}