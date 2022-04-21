using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.Jobs;

namespace Findx.Module.Admin.Scheduling
{
    [Job(Cron = "0 */10 * * * ?", Name = "定时同步缓存常量", Description = "")]
    public class RefreshConstantsTaskRunner : IJob
    {
        public Task<JobResult> RunAsync(IJobContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(JobResult.Success);
        }
    }
}

