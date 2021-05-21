using Findx.Extensions;
using System.Collections.Concurrent;
namespace Findx.Tasks.Scheduling
{
    /// <summary>
    /// 调度任务Wrapper字典
    /// </summary>
    public class SchedulerTaskWrapperDictionary
    {
        private static readonly ConcurrentDictionary<string, SchedulerTaskWrapper> _tasks = new ConcurrentDictionary<string, SchedulerTaskWrapper>();

        public static void Add(SchedulerTaskWrapper schedulerTask)
        {
            if (!_tasks.ContainsKey(schedulerTask.TaskFullName))
            {
                _tasks.TryAdd(schedulerTask.TaskFullName, schedulerTask);
            }
        }

        public static SchedulerTaskWrapper Get(string key)
        {
            return _tasks.GetOrDefault(key);
        }
    }
}
