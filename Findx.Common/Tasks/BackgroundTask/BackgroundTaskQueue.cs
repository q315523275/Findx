using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Tasks.BackgroundTask
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new ConcurrentQueue<Func<CancellationToken, Task>>();
        public Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            _workItems.TryDequeue(out var workItem);
            return Task.FromResult(workItem);
        }
        public Task EnqueueAsync(Func<CancellationToken, Task> workItem)
        {
            Check.NotNull(workItem, nameof(workItem));
            _workItems.Enqueue(workItem);
            return Task.CompletedTask;
        }
    }
}
