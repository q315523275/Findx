using System.Threading;
using System.Threading.Tasks;

namespace Findx.Scheduling
{
    /// <summary>
    /// 定义一个任务执行器
    /// </summary>
    public interface IScheduledTaskExecuter
    {
        /// <summary>
        /// 任务执行器调度执行
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task ExecuteAsync(TaskExecutionContext context, CancellationToken cancellationToken = default);
    }
}
