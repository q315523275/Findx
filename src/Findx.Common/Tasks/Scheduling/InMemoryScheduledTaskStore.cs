using Findx.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Findx.Tasks.Scheduling
{
    public class InMemoryScheduledTaskStore : IScheduledTaskStore
    {
        private readonly ConcurrentDictionary<Guid, SchedulerTaskInfo> _tasks;

        public InMemoryScheduledTaskStore()
        {
            _tasks = new ConcurrentDictionary<Guid, SchedulerTaskInfo>();
        }

        public Task DeleteAsync(Guid taskId)
        {
            _tasks.TryRemove(taskId, out _);
            return Task.CompletedTask;
        }

        public Task<SchedulerTaskInfo> FindAsync(Guid taskId)
        {
            return Task.FromResult(_tasks.GetOrDefault<Guid, SchedulerTaskInfo>(taskId));
        }

        public Task<List<SchedulerTaskInfo>> GetShouldRunTasksAsync(int maxResultCount)
        {
            var referenceTime = DateTimeOffset.UtcNow.LocalDateTime;
            var tasksThatShouldRun = _tasks.Values.Where(t => t.ShouldRun(referenceTime))
                                           .OrderBy(t => t.TryCount)
                                           .ThenBy(t => t.NextRunTime)
                                           .Take(maxResultCount)
                                           .ToList();
            return Task.FromResult(tasksThatShouldRun);
        }

        public Task<List<SchedulerTaskInfo>> GetTasksAsync()
        {
            return Task.FromResult(_tasks.Values.ToList());
        }

        public Task InsertAsync(SchedulerTaskInfo taskInfo)
        {
            _tasks[taskInfo.Id] = taskInfo;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(SchedulerTaskInfo taskInfo)
        {
            return Task.CompletedTask;
        }
    }
}
