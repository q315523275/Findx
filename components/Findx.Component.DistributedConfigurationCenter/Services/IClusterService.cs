using System.Threading;
using System.Threading.Tasks;
using Findx.Component.DistributedConfigurationCenter.Dtos;

namespace Findx.Component.DistributedConfigurationCenter.Services;

/// <summary>
///     集群服务
/// </summary>
public interface IClusterService
{
    /// <summary>
    ///     配置变更集群同步通知
    /// </summary>
    /// <param name="nodeInfo"></param>
    /// <param name="changeDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ConfigChangeClusterSyncNotifyAsync(string nodeInfo, ConfigDataChangeDto changeDto, CancellationToken cancellationToken = default);
}