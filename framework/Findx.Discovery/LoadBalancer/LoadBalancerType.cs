namespace Findx.Discovery.LoadBalancer;

/// <summary>
/// 负载均衡器类型
/// </summary>
public enum LoadBalancerType
{
    /// <summary>
    /// 随机
    /// </summary>
    Random,
    
    /// <summary>
    /// 轮询
    /// </summary>
    RoundRobin,
    
    /// <summary>
    /// 最少连接
    /// </summary>
    LeastConnection,

    // Hash,
    
    /// <summary>
    /// 无负载
    /// </summary>
    NoLoadBalancer
}