using System;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Discovery
{
    /// <summary>
    ///     服务注册
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IServiceRegistry<in T> : IDisposable
        where T : IServiceInstance
    {
        /// <summary>
        ///     注册服务
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> RegisterAsync(T registration, CancellationToken cancellationToken = default);

        /// <summary>
        ///     注销服务
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> DeregisterAsync(T registration, CancellationToken cancellationToken = default);

        /// <summary>
        ///     获取服务状态
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="status"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetStatusAsync(T registration, string status, CancellationToken cancellationToken = default);

        /// <summary>
        ///     设置服务状态
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> GetStatusAsync(T registration, CancellationToken cancellationToken = default);
    }
}