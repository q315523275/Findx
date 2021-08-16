using System;

namespace Findx.Tasks.Scheduling
{
    public class TaskExecutionContext
    {
        public IServiceProvider ServiceProvider { get; }

        public SchedulerTaskExecuteInfo ExecuteInfo { get; }

        public TaskExecutionContext(IServiceProvider serviceProvider, SchedulerTaskExecuteInfo executeInfo)
        {
            ServiceProvider = serviceProvider;
            ExecuteInfo = executeInfo;
        }
    }
}
