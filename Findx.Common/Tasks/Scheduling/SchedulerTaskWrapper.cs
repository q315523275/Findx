using Findx.Extensions;
using System;
namespace Findx.Tasks.Scheduling
{
    public class SchedulerTaskWrapper
    {
        public SchedulerTaskWrapper(Type taskHandlerType)
        {
            if (!typeof(IScheduledTask).IsAssignableFrom(taskHandlerType))
                throw new Exception("当前任务未实现IScheduledTask");

            TaskHandlerType = taskHandlerType;
            TaskFullName = taskHandlerType?.FullName;
            TaskName = taskHandlerType.GetAttribute<ScheduledAttribute>()?.Name ?? taskHandlerType.Name;
        }
        public SchedulerTaskWrapper(Type taskHandlerType, string name)
        {
            if (!typeof(IScheduledTask).IsAssignableFrom(taskHandlerType))
                throw new Exception("当前任务未实现IScheduledTask");

            TaskHandlerType = taskHandlerType;
            TaskFullName = taskHandlerType?.FullName;
            TaskName = name;
        }

        public string TaskName { get; }
        public string TaskFullName { get; }
        public Type TaskHandlerType { get; }
    }
}
