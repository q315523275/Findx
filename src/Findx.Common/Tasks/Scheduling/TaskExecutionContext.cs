using System;

namespace Findx.Tasks.Scheduling
{
    public class TaskExecutionContext
    {
        public IServiceProvider ServiceProvider { get; }

        public Guid TaskId { get; }

        public TaskExecutionContext(IServiceProvider serviceProvider, Guid taskId)
        {
            ServiceProvider = serviceProvider;
            TaskId = taskId;
        }
    }
}
