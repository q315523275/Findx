using System.Threading;
using System.Threading.Tasks;

namespace Findx.Tasks.Scheduling
{
    /// <summary>
    /// 定义一个任务执行器
    /// </summary>
    public interface IScheduledTaskExecuter
    {
        Task ExecuteAsync(TaskExecutionContext context, CancellationToken cancellationToken = default);
    }
}
