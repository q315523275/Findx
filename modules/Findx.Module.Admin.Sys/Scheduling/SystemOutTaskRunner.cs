using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.Scheduling;

namespace Findx.Module.Admin.Scheduling
{
    [Scheduled(Cron = "0 0 * * * ?", Name = "定时打印一句话")]
    public class SystemOutTaskRunner : IScheduledTask
    {
        public Task ExecuteAsync(ITaskContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

