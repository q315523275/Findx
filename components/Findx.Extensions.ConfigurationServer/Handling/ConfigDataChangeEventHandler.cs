using Findx.Extensions.ConfigurationServer.Dtos;
using Findx.Extensions.ConfigurationServer.Services;
using Findx.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.ConfigurationServer.Handling;

/// <summary>
///     配置数据变更事件处理器
/// </summary>
public class ConfigDataChangeEventHandler : IApplicationEventHandler<ConfigDataChangeEvent>
{
    private readonly IClusterService _clusterService;
    private readonly IDumpService _dumpService;
    private readonly IOptions<ConfigServerOptions> _options;
    private readonly ILogger<ConfigDataChangeEventHandler> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public ConfigDataChangeEventHandler(IOptions<ConfigServerOptions> options, IDumpService dumpService, IClusterService clusterService, ILogger<ConfigDataChangeEventHandler> logger)
    {
        _options = options;
        _dumpService = dumpService;
        _clusterService = clusterService;
        _logger = logger;
    }

    /// <summary>
    ///     处理
    /// </summary>
    public async Task HandleAsync(ConfigDataChangeEvent applicationEvent, CancellationToken cancellationToken = default)
    {
        var changeDataDto = new ConfigDataChangeDto
        {
            AppId = applicationEvent.AppId,
            DataId = applicationEvent.DataId,
            DataType = applicationEvent.DataType,
            Content = applicationEvent.Content,
            Environment = applicationEvent.Environment,
            Version = applicationEvent.Version
        };

        //  先通知自己（本地长轮询客户端）
        await _dumpService.DumpAsync(changeDataDto, cancellationToken);

        //  通知集群其他节点：节点规范化比较（去尾部斜杠、忽略大小写），单节点失败不阻断整体发布
        var currentNode = _options.Value.CurrentNode?.TrimEnd('/');
        var notifyTasks = new List<Task>();
        foreach (var nodeInfo in _options.Value.ClusterNodes)
        {
            var node = nodeInfo?.TrimEnd('/');
            if (string.IsNullOrEmpty(node) || string.Equals(node, currentNode, StringComparison.OrdinalIgnoreCase))
                continue;

            notifyTasks.Add(NotifyNodeAsync(node, changeDataDto, cancellationToken));
        }

        await Task.WhenAll(notifyTasks);
    }

    private async Task NotifyNodeAsync(string node, ConfigDataChangeDto changeDataDto, CancellationToken cancellationToken)
    {
        try
        {
            await _clusterService.ConfigChangeClusterSyncNotifyAsync(node, changeDataDto, cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            //  集群节点失败不影响配置已本地生效；记录告警供运维处置
            _logger.LogWarning(ex, "集群节点 {Node} 配置变更通知失败，配置已本地生效", node);
        }
    }
}
