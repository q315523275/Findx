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
        {
            //  未注册的实现必须显式失败：上下文默认结果是成功，静默返回会把"没执行"上报成"执行成功"，
            //  分布式调度侧因此会清零重试计数并认为该周期已完成
            _logger.LogWarning("作业 {Name} 的实现类型 {Type} 未在本节点注册，拒绝执行", context.JobExecuteInfo.Name, context.JobExecuteInfo.FullName);
            context.JobResult = new JobResult
            {
                IsSuccess = false,
                Message = $"执行节点未注册该作业实现：{context.JobExecuteInfo.FullName}"
            };
            return;
        }

        var job = context.ServiceProvider.GetRequiredService(jobType) as IJob;
        if (job == null)
        {
            _logger.LogWarning("作业 {Name} 的类型 {Type} 未实现 IJob，拒绝执行", context.JobExecuteInfo.Name, context.JobExecuteInfo.FullName);
            context.JobResult = new JobResult
            {
                IsSuccess = false,
                Message = $"作业实现类型无法解析为 IJob：{context.JobExecuteInfo.FullName}"
            };
            return;
        }

        _logger.LogDebug("作业 {Name} 记录 {Id} 规定 {RunTime} 实际 {Now}", context.JobExecuteInfo.Name, context.JobExecuteInfo.Id, context.JobExecuteInfo.RunTime, DateTimeOffset.Now);
        try
        {
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