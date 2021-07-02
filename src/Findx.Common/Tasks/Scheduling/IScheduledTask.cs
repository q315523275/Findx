using System.Threading;
using System.Threading.Tasks;

namespace Findx.Tasks.Scheduling
{
    /// <summary>
    /// 定义一个调度任务
    /// </summary>
    public interface IScheduledTask
    {
        /// <summary>
        /// 执行任务内容
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task ExecuteAsync(ITaskContext context, CancellationToken cancellationToken = default);
    }
}
