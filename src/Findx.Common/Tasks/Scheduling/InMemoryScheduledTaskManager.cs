using Findx.Extensions;
using Findx.Serialization;
using System;
using System.Threading.Tasks;

namespace Findx.Tasks.Scheduling
{
    public class InMemoryScheduledTaskManager : IScheduledTaskManager
    {
        private readonly IScheduledTaskStore _taskStore;
        private readonly IJsonSerializer _serializer;
        private readonly SchedulerTaskWrapperDictionary _dict;

        public InMemoryScheduledTaskManager(IScheduledTaskStore taskStore, IJsonSerializer serializer, SchedulerTaskWrapperDictionary dict)
        {
            _taskStore = taskStore;
            _serializer = serializer;
            _dict = dict;
        }

        public async Task<string> EnqueueAsync<TTaskHandler>(object taskArgs, TimeSpan? delay = null) where TTaskHandler : IScheduledTask
        {
            var taskArgsType = typeof(TTaskHandler);
            var taskInfo = new SchedulerTaskInfo
            {
                CreateTime = DateTimeOffset.UtcNow.LocalDateTime,
                IsEnable = true,
                IsSingle = true,
                NextRunTime = DateTimeOffset.UtcNow.LocalDateTime,
                Id = Guid.NewGuid(),
                TaskArgs = _serializer.Serialize(taskArgs ?? new { }),
                TaskName = taskArgsType.Name,
                TaskFullName = taskArgsType.FullName,
                TryCount = 0,
            };

            if (delay.HasValue)
            {
                taskInfo.NextRunTime = DateTimeOffset.UtcNow.Add(delay.Value).LocalDateTime;
            }

            AddWrapper(taskArgsType);

            await _taskStore.InsertAsync(taskInfo);

            return taskInfo.Id.ToString();
        }

        public async Task<string> EnqueueAsync<TTaskHandler>(object taskArgs, DateTime? dateTime = null) where TTaskHandler : IScheduledTask
        {
            var taskArgsType = typeof(TTaskHandler);
            var taskInfo = new SchedulerTaskInfo
            {
                CreateTime = DateTimeOffset.UtcNow.LocalDateTime,
                IsEnable = true,
                IsSingle = true,
                NextRunTime = dateTime ?? DateTimeOffset.UtcNow.LocalDateTime,
                Id = Guid.NewGuid(),
                TaskArgs = _serializer.Serialize(taskArgs ?? new { }),
                TaskName = taskArgsType.Name,
                TaskFullName = taskArgsType.FullName,
                TryCount = 0,
            };

            AddWrapper(taskArgsType);

            await _taskStore.InsertAsync(taskInfo);

            return taskInfo.Id.ToString();
        }

        public async Task<string> ScheduleAsync<TTaskHandler>(object taskArgs, TimeSpan delay) where TTaskHandler : IScheduledTask
        {
            var taskArgsType = typeof(TTaskHandler);
            var taskInfo = new SchedulerTaskInfo
            {
                CreateTime = DateTimeOffset.UtcNow.LocalDateTime,
                FixedDelay = delay.TotalSeconds,
                IsEnable = true,
                IsSingle = false,
                NextRunTime = DateTimeOffset.UtcNow.Add(delay).LocalDateTime,
                Id = Guid.NewGuid(),
                TaskArgs = _serializer.Serialize(taskArgs ?? new { }),
                TaskName = taskArgsType.Name,
                TaskFullName = taskArgsType.FullName,
                TryCount = 0,
            };

            AddWrapper(taskArgsType);

            await _taskStore.InsertAsync(taskInfo);

            return taskInfo.Id.ToString();
        }

        public async Task<string> ScheduleAsync<TTaskHandler>(object taskArgs, string cronExpression) where TTaskHandler : IScheduledTask
        {
            var taskArgsType = typeof(TTaskHandler);
            var taskInfo = new SchedulerTaskInfo
            {
                CreateTime = DateTimeOffset.UtcNow.LocalDateTime,
                CronExpress = cronExpression,
                IsEnable = true,
                IsSingle = false,
                NextRunTime = Utils.Cron.GetNextOccurrence(cronExpression),
                Id = Guid.NewGuid(),
                TaskArgs = _serializer.Serialize(taskArgs ?? new { }),
                TaskName = taskArgsType.Name,
                TaskFullName = taskArgsType.FullName,
                TryCount = 0,
            };

            AddWrapper(taskArgsType);

            await _taskStore.InsertAsync(taskInfo);

            return taskInfo.Id.ToString();
        }

        public async Task<string> ScheduleAsync(SchedulerTaskWrapper wrapper, TimeSpan delay)
        {
            var taskInfo = new SchedulerTaskInfo
            {
                CreateTime = DateTimeOffset.UtcNow.LocalDateTime,
                FixedDelay = delay.TotalSeconds,
                IsEnable = true,
                IsSingle = false,
                NextRunTime = DateTimeOffset.UtcNow.Add(delay).LocalDateTime,
                Id = Guid.NewGuid(),
                TaskArgs = "{}",
                TaskName = wrapper.TaskName,
                TaskFullName = wrapper.TaskFullName,
                TryCount = 0,
            };

            AddWrapper(wrapper);

            await _taskStore.InsertAsync(taskInfo);

            return taskInfo.Id.ToString();
        }

        public async Task<string> ScheduleAsync(SchedulerTaskWrapper wrapper, string cronExpression)
        {
            var taskInfo = new SchedulerTaskInfo
            {
                CreateTime = DateTimeOffset.UtcNow.LocalDateTime,
                CronExpress = cronExpression,
                IsEnable = true,
                IsSingle = false,
                NextRunTime = Utils.Cron.GetNextOccurrence(cronExpression),
                Id = Guid.NewGuid(),
                TaskArgs = "{}",
                TaskName = wrapper.TaskName,
                TaskFullName = wrapper.TaskFullName,
                TryCount = 0,
            };

            AddWrapper(wrapper);

            await _taskStore.InsertAsync(taskInfo);

            return taskInfo.Id.ToString();
        }

        public Task<string> ScheduleAsync(SchedulerTaskWrapper wrapper)
        {
            var attribute = wrapper.TaskHandlerType.GetAttribute<ScheduledAttribute>();

            Check.NotNull(attribute, nameof(attribute));

            if (attribute.FixedDelay > 0)
            {
                return ScheduleAsync(wrapper, TimeSpan.FromSeconds(attribute.FixedDelay));
            }

            if (!attribute.Cron.IsNullOrWhiteSpace())
            {
                return ScheduleAsync(wrapper, attribute.Cron);
            }

            return Task.FromResult("-1");
        }

        private void AddWrapper(Type handerType)
        {
            var wrapper = new SchedulerTaskWrapper(handerType);
            _dict.TryAdd(wrapper.TaskFullName, wrapper);
        }
        private void AddWrapper(SchedulerTaskWrapper wrapper)
        {
            _dict.TryAdd(wrapper.TaskFullName, wrapper);
        }
    }
}
