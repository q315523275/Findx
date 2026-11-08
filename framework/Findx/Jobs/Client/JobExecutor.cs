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
        _logger.LogDebug("作业 {Name} 记录 {Id} 规定 {RunTime} 实际 {Now}", context.JobExecuteInfo.Name, context.JobExecuteInfo.Id, context.JobExecuteInfo.RunTime, DateTimeOffset.Now);
        try
        {
            if (job != null)
                context.JobResult = await job.RunAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            //  作业被取消（如应用关闭、Host 停止）不作为失败处理，不向外抛出
            if (ex is OperationCanceledException)
            {
                context.JobResult = JobResult.CancelledWithMessage(ex.Message);
                return;
            }

            _logger.LogError(ex, "作业 {Name} 执行记录 {Id} 执行失败", context.JobExecuteInfo.Name, context.JobExecuteInfo.Id);

            //  置位失败结果并重新抛出，交由上层调度器（JobExecuteDispatcher）统一通知异常管线
            context.JobResult = JobResult.FromException(ex);
            throw;
        }
    }
}