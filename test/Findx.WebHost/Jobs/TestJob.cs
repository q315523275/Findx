using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.Jobs;
using Findx.Jobs.Client;
using Findx.Jobs.Common;

namespace Findx.WebHost.Jobs;

/// <summary>
///     test job
/// </summary>
[Job(Cron = "*/5 * * * * ?", Name = "演示自动任务")]
public class TestJob: IJob
{
    public Task<JobResult> RunAsync(JobExecuteContext context, CancellationToken cancellationToken = default)
    {
        //Console.WriteLine(DateTime.Now);
        return Task.FromResult(JobResult.Success);
    }
}