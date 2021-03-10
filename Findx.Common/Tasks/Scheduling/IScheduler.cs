using System.Threading;
using System.Threading.Tasks;

namespace Findx.Tasks.Scheduling
{
    /// <summary>
    /// 调度器
    /// </summary>
    public interface IScheduler
    {
        /// <summary>
        /// 启动调度器
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StartAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// 停止调度器
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
