using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.Jobs;

namespace Findx.Module.Admin.Scheduling
{
    [Job(Cron = "0 0 2 30 * ?", Name = "定时打印一句话")]
    public class SystemOutTaskRunner : IJob
    {
        public Task<JobResult> RunAsync(IJobContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(JobResult.Success);
        }
    }
}

