using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.Discovery.Abstractions;

namespace Findx.Discovery.LoadBalancer;

/// <summary>
///     负载平衡器
/// </summary>
public interface ILoadBalancer
{
    /// <summary>
    ///     负载平衡器名称
    /// </summary>
    LoadBalancerType Name { get; }

    /// <summary>
    ///     解析服务实例
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IServiceEndPoint> ResolveServiceEndPointAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     更新实例状态
    /// </summary>
    /// <param name="serviceEndPoint"></param>
    /// <param name="responseTime"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateStatsAsync(IServiceEndPoint serviceEndPoint, TimeSpan responseTime, CancellationToken cancellationToken = default);
}