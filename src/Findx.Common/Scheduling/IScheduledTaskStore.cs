using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Findx.Scheduling
{
    /// <summary>
    /// 定义一个调度任务存储器
    /// </summary>
    public interface IScheduledTaskStore
    {
        /// <summary>
        /// 存储任务信息
        /// </summary>
        /// <param name="taskInfo"></param>
        /// <returns></returns>
        Task InsertAsync(SchedulerTaskInfo taskInfo);
        /// <summary>
        /// 删除任务信息
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        Task DeleteAsync(Guid taskId);
        /// <summary>
        /// 更新任务信息
        /// </summary>
        /// <param name="taskInfo"></param>
        /// <returns></returns>
        Task UpdateAsync(SchedulerTaskInfo taskInfo);
        /// <summary>
        /// 查询任务信息
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        Task<SchedulerTaskInfo> FindAsync(Guid taskId);
        /// <summary>
        /// 查询可以执行任务列表
        /// </summary>
        /// <param name="maxResultCount"></param>
        /// <returns></returns>
        Task<IEnumerable<SchedulerTaskInfo>> GetShouldRunTasksAsync(int maxResultCount);
        /// <summary>
        /// 查询所有任务
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<SchedulerTaskInfo>> GetTasksAsync();
    }
}
