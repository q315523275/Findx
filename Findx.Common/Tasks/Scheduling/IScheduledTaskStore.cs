using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Findx.Tasks.Scheduling
{
    /// <summary>
    /// 定义一个调度任务存储器
    /// </summary>
    public interface IScheduledTaskStore
    {
        Task InsertAsync(SchedulerTaskInfo taskInfo);
        Task DeleteAsync(Guid taskId);
        Task UpdateAsync(SchedulerTaskInfo taskInfo);
        Task<SchedulerTaskInfo> FindAsync(Guid taskId);
        Task<List<SchedulerTaskInfo>> GetShouldRunTasksAsync(int maxResultCount);
        Task<List<SchedulerTaskInfo>> GetTasksAsync();
    }
}
