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
    ///     集群节点互信令牌（集群变更通知鉴权，配置后 ClusterController 校验 X-Cluster-Token）
    /// </summary>
    public string ClusterToken { set; get; }

    /// <summary>
    ///     集群通知协议（http/https），节点配置为 host:port
    /// </summary>
    public string ClusterScheme { set; get; } = "http";

    /// <summary>
    ///     客户端变更监听长轮询超时（秒）
    /// </summary>
    public int ClientLongPollTimeout { set; get; } = 30;

    /// <summary>
    ///     客户端签名防重放时间窗口（分钟）
    /// </summary>
    public int ReplayWindowMinutes { set; get; } = 5;

    /// <summary>
    ///     this
    /// </summary>
    public ConfigServerOptions Value => this;
}