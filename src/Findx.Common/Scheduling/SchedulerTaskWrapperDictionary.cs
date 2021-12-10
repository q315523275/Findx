using System;
using System.Collections.Concurrent;
namespace Findx.Scheduling
{
    /// <summary>
    /// 调度任务Wrapper字典
    /// </summary>
    public class SchedulerTaskWrapperDictionary : ConcurrentDictionary<string, SchedulerTaskWrapper>, IDisposable
    {
        public void Dispose()
        {
            this.Clear();
        }
    }
}
