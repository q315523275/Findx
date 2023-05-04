using Findx.Messaging;
using Findx.Module.ConfigService.Dtos;
using Findx.Module.ConfigService.Services;
using Microsoft.Extensions.Options;

namespace Findx.Module.ConfigService.Handling;

/// <summary>
///     配置数据变更事件处理器
/// </summary>
public class ConfigDataChangeEventHandler : IApplicationEventHandler<ConfigDataChangeEvent>
{
    private readonly IClusterService _clusterService;
    private readonly IDumpService _dumpService;
    private readonly IOptions<ConfigServiceOptions> _options;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="dumpService"></param>
    /// <param name="clusterService"></param>
    public ConfigDataChangeEventHandler(IOptions<ConfigServiceOptions> options, IDumpService dumpService,
        IClusterService clusterService)
    {
        _options = options;
        _dumpService = dumpService;
        _clusterService = clusterService;
    }

    /// <summary>
    ///     处理
    /// </summary>
    /// <param name="applicationEvent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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
        // 先通知自己
        await _dumpService.DumpAsync(changeDataDto, cancellationToken);
        // 通知集群其他节点
        var notifyTasks = new List<Task>();
        foreach (var nodeInfo in _options.Value.ClusterNodes)
            if (nodeInfo != _options.Value.CurrentNode)
                notifyTasks.Add(
                    _clusterService.ConfigChangeClusterSyncNotifyAsync(nodeInfo, changeDataDto, cancellationToken));
        await Task.WhenAll(notifyTasks);
    }
}