using Findx.Extensions;
using Findx.Jobs;

namespace Findx.Module.EleAdmin.Jobs;

public class TestJobTask: IJob
{
    public Task<JobResult> RunAsync(IJobContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"{DateTime.Now}==={context.JobName}==={context.ExecutionId}==={context.Parameter.ToJson()}");
        return Task.FromResult(JobResult.Success);
    }
}