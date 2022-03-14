﻿using Findx.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Findx.Scheduling
{
    public class InMemoryScheduledTaskStore : IScheduledTaskStore
    {
        private readonly Dictionary<Guid, SchedulerTaskInfo> _tasks;

        public InMemoryScheduledTaskStore()
        {
            _tasks = new Dictionary<Guid, SchedulerTaskInfo>();
        }

        public Task DeleteAsync(Guid taskId)
        {
            _tasks.Remove(taskId);
            return Task.CompletedTask;
        }

        public Task<SchedulerTaskInfo> FindAsync(Guid taskId)
        {
            return Task.FromResult(_tasks.GetOrDefault<Guid, SchedulerTaskInfo>(taskId));
        }
        /// <summary>
        /// 查询待执行任务
        /// </summary>
        /// <param name="maxResultCount"></param>
        /// <returns></returns>
        public Task<IEnumerable<SchedulerTaskInfo>> GetShouldRunTasksAsync(int maxResultCount)
        {
            var referenceTime = DateTimeOffset.UtcNow.LocalDateTime;
            var tasksThatShouldRun = _tasks.Select(x => x.Value)
                                           .Where(t => t.ShouldRun(referenceTime))
                                           .OrderBy(t => t.TryCount)
                                           .ThenBy(t => t.NextRunTime)
                                           .Take(maxResultCount);
            return Task.FromResult(tasksThatShouldRun);
        }

        public Task<IEnumerable<SchedulerTaskInfo>> GetTasksAsync()
        {
            return Task.FromResult(_tasks.Select(x => x.Value));
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
