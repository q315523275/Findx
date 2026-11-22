using Findx.Extensions.WorkflowCore.ServiceDefaults;
using Findx.Jobs.Client;
using Findx.Jobs.Common;

namespace Findx.Extensions.WorkflowCore.Jobs;

/// <summary>
///     工作流任务提醒与超时扫描作业
///     <para>实现 IJob，由 Findx.Jobs 自动注册；用内存版还是分布式版由宿主配置 Findx:Jobs 决定</para>
/// </summary>
[Job(Cron = "0 * * * * ?", Name = "工作流任务超时扫描", Description = "扫描审批任务的临近提醒与到期处理")]
public class FlwTaskTimeoutJob : IJob
{
    private readonly FlwTimeoutHandler _handler;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="handler"></param>
    public FlwTaskTimeoutJob(FlwTimeoutHandler handler)
    {
        _handler = handler;
    }

    /// <summary>
    ///     执行
    /// </summary>
    public async Task<JobResult> RunAsync(JobExecuteContext context, CancellationToken cancellationToken = default)
    {
        var result = await _handler.HandleAsync(cancellationToken);
        return new JobResult { IsSuccess = true, Message = result.ToString() };
    }
}
