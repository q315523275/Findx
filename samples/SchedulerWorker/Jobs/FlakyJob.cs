using Findx.Jobs.Client;
using Findx.Jobs.Common;

namespace Findx.Samples.SchedulerWorker.Jobs;

/// <summary>
///     演示任务二：必然失败。
///     <para>用途是看完整失败链路：执行端上报失败 → 调度端按指数退避重排（RetryAtTime）→
///     达到 MaxRetryCount 后任务被隔离为 Dead → 管理面 POST jobs/release 人工放行。</para>
/// </summary>
[Job(Name = "演示-失败重试", Description = "每次执行都失败，用于演示退避重试与死信隔离")]
public class FlakyJob : IJob
{
    public Task<JobResult> RunAsync(JobExecuteContext context, CancellationToken cancellationToken = default)
    {
        var log = context.LogRecorder;
        log.Warn("[FlakyJob] 本任务按设计每次都会失败，用于验证重试与死信隔离");

        //  失败消息会落到 scheduler_execution.Message，可在管理面 executions/{traceId} 直接看到
        return Task.FromResult(new JobResult
        {
            IsSuccess = false,
            Message = $"演示失败：{DateTimeOffset.Now:HH:mm:ss}，节点 {Environment.MachineName}"
        });
    }
}
