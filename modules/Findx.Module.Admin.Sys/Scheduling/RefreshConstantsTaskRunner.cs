using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.Scheduling;

namespace Findx.Module.Admin.Scheduling
{
    [Scheduled(Cron = "0 0/10 * * * ?", Name = "定时同步缓存常量")]
    public class RefreshConstantsTaskRunner : IScheduledTask
    {
        public Task ExecuteAsync(ITaskContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

