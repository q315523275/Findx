using Findx.Jobs.Client;
using Findx.Jobs.Common;

namespace Findx.Samples.SchedulerWorker.Jobs;

/// <summary>
///     演示任务一：正常完成。
///     <para>重点展示执行上下文里的 <see cref="JobExecuteContext.LogRecorder" />：
///     这些信息会被组件采集、分片上报，最终能在管理面按 TraceId 查看。</para>
/// </summary>
[Job(Name = "演示-问候", Description = "每 10 秒成功一次的示例任务")]
public class HelloJob : IJob
{
    public async Task<JobResult> RunAsync(JobExecuteContext context, CancellationToken cancellationToken = default)
    {
        var log = context.LogRecorder;
        var info = context.JobExecuteInfo;

        log.Info($"[HelloJob] 开始执行，任务={info.Name}，参数={info.Parameter ?? "(空)"}，节点={Environment.MachineName}");

        for (var step = 1; step <= 3; step++)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(300), cancellationToken);
            log.Info($"[HelloJob] 第 {step}/3 步完成");
        }

        log.Info("[HelloJob] 全部步骤完成，返回成功");
        return JobResult.Success;
    }
}
