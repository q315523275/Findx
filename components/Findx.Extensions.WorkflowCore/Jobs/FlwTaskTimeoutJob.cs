using Findx.Extensions.WorkflowCore.Services;
using Findx.Jobs.Client;
using Findx.Jobs.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.Extensions.WorkflowCore.Jobs;

/// <summary>
///     工作流任务提醒与超时扫描作业
///     <para>实现 IJob，由 Findx.Jobs 自动注册；用内存版还是分布式版由宿主配置 Findx:Jobs 决定</para>
/// </summary>
[Job(Cron = "0 * * * * ?", Name = "工作流任务超时扫描", Description = "扫描审批任务的临近提醒与到期处理")]
public class FlwTaskTimeoutJob : IJob
{
    /// <summary>
    ///     执行
    /// </summary>
    public async Task<JobResult> RunAsync(JobExecuteContext context, CancellationToken cancellationToken = default)
    {
        using var scope = context.ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<FlwTimeoutHandler>();
        var result = await handler.HandleAsync(cancellationToken);
        return new JobResult { IsSuccess = true, Message = result.ToString() };
    }
}
