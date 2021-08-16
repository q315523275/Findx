using System;
using System.Threading.Tasks;

namespace Findx.Discovery
{
    /// <summary>
    /// 负载平衡器
    /// </summary>
    public interface ILoadBalancer
    {
        /// <summary>
        /// 负载平衡器名称
        /// </summary>
        LoadBalancerType Name { get; }

        /// <summary>
        /// 解析服务实例
        /// </summary>
        /// <returns></returns>
        Task<IServiceInstance> ResolveServiceInstanceAsync();

        /// <summary>
        /// 更新实例状态
        /// </summary>
        /// <param name="serviceInstance"></param>
        /// <param name="responseTime"></param>
        /// <returns></returns>
        Task UpdateStatsAsync(IServiceInstance serviceInstance, TimeSpan responseTime);
    }
}
