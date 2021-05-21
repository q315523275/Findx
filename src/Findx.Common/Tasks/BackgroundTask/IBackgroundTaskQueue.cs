using System;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Tasks.BackgroundTask
{
    public interface IBackgroundTaskQueue
    {
        Task EnqueueAsync(Func<CancellationToken, Task> workItem);
        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
}
