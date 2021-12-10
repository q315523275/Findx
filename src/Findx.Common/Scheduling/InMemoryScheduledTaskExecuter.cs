using Findx.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Scheduling
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
            var storage = context.ServiceProvider.GetRequiredService<IScheduledTaskStore>();
            var jsonSerializer = context.ServiceProvider.GetRequiredService<IJsonSerializer>();
            var scheduledDict = context.ServiceProvider.GetRequiredService<SchedulerTaskWrapperDictionary>();

            var taskInfo = await storage.FindAsync(context.ExecuteInfo.TaskId);

            scheduledDict.TryGetValue(context.ExecuteInfo.TaskFullName, out var schedulerTaskWrapper);
            Check.NotNull(schedulerTaskWrapper, nameof(schedulerTaskWrapper));

            // 任务参数
            var taskArgsInfo = jsonSerializer.Deserialize<Dictionary<string, object>>(context.ExecuteInfo.TaskArgs ?? "{}");
            // 获取任务实例
            var scheduledTask = (IScheduledTask)context.ServiceProvider.GetService(schedulerTaskWrapper.TaskHandlerType);
            // 创建任务上下文
            var taskContext = new TaskContext(context.ServiceProvider, taskArgsInfo, context.ExecuteInfo.TaskId);

            var executeTime = DateTime.Now;

            try
            {
                context.ExecuteInfo.ExecuteTime = executeTime;
                context.ExecuteInfo.DelayRunTime = (executeTime - context.ExecuteInfo.TaskTime).TotalSeconds;
                context.ExecuteInfo.Status = 1;

                await scheduledTask.ExecuteAsync(taskContext, cancellationToken);

                context.ExecuteInfo.Status = 2;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"调度任务({context.ExecuteInfo.TaskName}-{context.ExecuteInfo.ExecuteId})执行异常");
                context.ExecuteInfo.Status = 3;
            }
            finally
            {
                // 固定间隔时间任务
                if (taskInfo.FixedDelay > 0)
                    taskInfo.Increment();

                context.ExecuteInfo.RunTime = (DateTime.Now - executeTime).TotalSeconds;

                // 更新上报任务执行信息、任务主信息
            }
            // Console.WriteLine(jsonSerializer.Serialize(context.ExecuteInfo));
        }
    }
}
