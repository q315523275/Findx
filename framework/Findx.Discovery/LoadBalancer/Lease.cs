using Findx.Discovery.Abstractions;

namespace Findx.Discovery.LoadBalancer;

/// <summary>
/// 服务负载计算-连接少优先
/// </summary>
public class Lease
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="serviceEndPoint"></param>
    /// <param name="connections"></param>
    public Lease(IServiceEndPoint serviceEndPoint, int connections)
    {
        ServiceEndPoint = serviceEndPoint;
        Connections = connections;
    }

    /// <summary>
    /// 服务实例
    /// </summary>
    public IServiceEndPoint ServiceEndPoint { get; private set; }
        
    /// <summary>
    /// 连接数
    /// </summary>
    public int Connections { get; private set; }
}