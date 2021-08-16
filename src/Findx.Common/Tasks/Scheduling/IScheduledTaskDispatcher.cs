using System.Threading;
using System.Threading.Tasks;

namespace Findx.Tasks.Scheduling
{
    /// <summary>
    /// 调度任务执行调度器
    /// </summary>
    public interface IScheduledTaskDispatcher
    {
        /// <summary>
        /// 推送执行任务
        /// </summary>
        /// <param name="executeInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task EnqueueToPublishAsync(SchedulerTaskExecuteInfo executeInfo, CancellationToken cancellationToken = default);

        /// <summary>
        /// 推送任务执行
        /// </summary>
        /// <param name="executeInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task EnqueueToExecuteAsync(SchedulerTaskExecuteInfo executeInfo, CancellationToken cancellationToken = default);
    }
}
