using System.Threading;
using System.Threading.Tasks;
using Findx.Jobs.Client;
using Findx.Jobs.Common;

namespace Findx.WebHost.Jobs;

/// <summary>
///     test job
/// </summary>
[Job(Cron = "*/6 * * * * ?", Name = "演示自动任务2")]
public class TestJob2: IJob
{
    public Task<JobResult> RunAsync(JobExecuteContext context, CancellationToken cancellationToken = default)
    {
        //Console.WriteLine(DateTime.Now);
        return Task.FromResult(JobResult.Success);
    }
}