using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Discovery.Abstractions;

/// <summary>
///     服务实例提供器
/// </summary>
public interface IServiceEndPointProvider
{
    /// <summary>
    ///     查询服务有效实例端点集合
    /// </summary>
    /// <param name="serviceName">服务名</param>
    /// <param name="group">组</param>
    /// <param name="passingOnly">是否仅探测通过</param>
    /// <param name="tag">服务标签</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyList<IServiceEndPoint>> GetServiceEndPointsAsync(string serviceName, string group = null, bool passingOnly = true, string tag = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     查询组下所有服务
    /// </summary>
    /// <param name="group"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<string>> GetServicesAsync(string group = null, CancellationToken cancellationToken = default);
}