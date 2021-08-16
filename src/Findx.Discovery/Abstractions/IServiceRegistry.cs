using System;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Discovery
{
    /// <summary>
    /// 服务注册
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IServiceRegistry<in T> : IDisposable
        where T : IServiceInstance
    {
        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> Register(T registration, CancellationToken cancellationToken = default);

        /// <summary>
        /// 注销服务
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> Deregister(T registration, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取服务状态
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="status"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetStatus(T registration, string status, CancellationToken cancellationToken = default);

        /// <summary>
        /// 设置服务状态
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> GetStatus(T registration, CancellationToken cancellationToken = default);
    }
}
