using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.Jobs;

namespace Findx.WebHost.Jobs;

/// <summary>
///     test job
/// </summary>
[Job(Cron = "0/10 * * * * ?", Name = "演示自动任务")]
public class TestJob: IJob
{
    public Task<JobResult> RunAsync(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(DateTime.Now);
        return Task.FromResult(JobResult.Success);
    }
}