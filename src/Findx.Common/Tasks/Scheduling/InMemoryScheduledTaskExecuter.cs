﻿using Findx.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Tasks.Scheduling
{
    public class InMemoryScheduledTaskExecuter : IScheduledTaskExecuter
    {
        private readonly ILogger<InMemoryScheduledTaskExecuter> _logger;

        public InMemoryScheduledTaskExecuter(ILogger<InMemoryScheduledTaskExecuter> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync(TaskExecutionContext context, CancellationToken cancellationToken = default)
        {
            var store = context.ServiceProvider.GetRequiredService<IScheduledTaskStore>();
            var jsonSerializer = context.ServiceProvider.GetRequiredService<IJsonSerializer>();
            var scheduledDict = context.ServiceProvider.GetRequiredService<SchedulerTaskWrapperDictionary>();

            var taskInfo = await store.FindAsync(context.TaskId);
            Check.NotNull(taskInfo, nameof(taskInfo));

            scheduledDict.TryGetValue(taskInfo.TaskFullName, out var schedulerTaskWrapper);
            Check.NotNull(schedulerTaskWrapper, nameof(schedulerTaskWrapper));

            // 任务参数
            var taskArgsInfo = jsonSerializer.Deserialize<Dictionary<string, object>>(taskInfo.TaskArgs ?? "{}");
            // 获取任务实例
            var scheduledTask = context.ServiceProvider.GetService(schedulerTaskWrapper.TaskHandlerType);
            // 创建任务上下文
            var taskContext = new TaskContext(context.ServiceProvider, taskArgsInfo, context.TaskId);

            try
            {
                await (Task)schedulerTaskWrapper.TaskHandlerType.GetMethod("ExecuteAsync").Invoke(scheduledTask, new object[] { taskContext, cancellationToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"调度任务:{taskInfo},执行异常");
            }
            // 待需要可以增加任务执行日志

        }
    }
}
