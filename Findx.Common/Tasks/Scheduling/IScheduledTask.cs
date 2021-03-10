using System.Threading;
using System.Threading.Tasks;

namespace Findx.Tasks.Scheduling
{
    /// <summary>
    /// 定义一个调度任务
    /// </summary>
    /// <typeparam name="TTaskArgs"></typeparam>
    public interface IScheduledTask
    {
        Task ExecuteAsync(ITaskContext context, CancellationToken cancellationToken = default);
    }
}
