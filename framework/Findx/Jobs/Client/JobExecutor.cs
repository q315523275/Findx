using System.Threading.Tasks;
using Findx.Jobs.Common;

namespace Findx.Jobs.Client;

/// <summary>
///     任务执行器
/// </summary>
public class JobExecutor: IJobExecutor
{
    private readonly JobWorkSet _jobWorkSet;
    private readonly ILogger<JobExecutor> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="jobWorkSet"></param>
    /// <param name="logger"></param>
    public JobExecutor(JobWorkSet jobWorkSet, ILogger<JobExecutor> logger)
    {
        _jobWorkSet = jobWorkSet;
        _logger = logger;
    }

    /// <summary>
    ///     执行
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ExecuteAsync(JobExecuteContext context, CancellationToken cancellationToken = default)
    {
        if (!_jobWorkSet.TryGetValue(context.JobExecuteInfo.FullName, out var jobType))
            return;
        var job = context.ServiceProvider.GetRequiredService(jobType) as IJob;
        Console.WriteLine($"作业 {context.JobExecuteInfo.Name} 记录 {context.JobExecuteInfo.Id} 规定 {context.JobExecuteInfo.RunTime} 实际 {DateTimeOffset.Now}");
        try
        {
            if (job != null) 
                context.JobResult = await job.RunAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "作业 {Name} 执行记录 {Id} 执行失败", context.JobExecuteInfo.Name, context.JobExecuteInfo.Id);
        }
    }
}