using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Discovery.Abstractions;

/// <summary>
///     发现客户端
/// </summary>
public interface IDiscoveryClient : IServiceEndPointProvider
{
    /// <summary>
    ///     提供器名称
    /// </summary>
    string ProviderName { get; }
    
    /// <summary>
    ///     查询组下所有服务有效实例端点集合
    /// </summary>
    /// <param name="group"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IList<IServiceEndPoint>> GetAllEndPointsAsync(string group = null, CancellationToken cancellationToken = default);
}