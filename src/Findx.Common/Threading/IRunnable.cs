using System.Threading.Tasks;

namespace Findx.Threading
{
    /// <summary>
    /// 线程启动停止服务接口
    /// </summary>
    public interface IRunnable
	{
        /// <summary>
        /// 开始服务
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 停止服务
        /// </summary>
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}

