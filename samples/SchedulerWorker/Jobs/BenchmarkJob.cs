using Findx.Jobs.Client;
using Findx.Jobs.Common;

namespace Findx.Samples.SchedulerWorker.Jobs;

/// <summary>
///     压测任务：不做业务，仅按参数模拟耗时，用于测量调度端「扫描 → 领取 → 投递」
///     与执行端「受理 → 排队 → 执行 → 回调」的吞吐上限。
///     <para>参数即模拟耗时（毫秒），为空或 0 时立即返回。不写执行日志，避免日志上报喧宾夺主。</para>
/// </summary>
[Job(Name = "压测-空跑", Description = "立即返回；参数可指定模拟耗时（毫秒）")]
public class BenchmarkJob : IJob
{
    public async Task<JobResult> RunAsync(JobExecuteContext context, CancellationToken cancellationToken = default)
    {
        //  参数为毫秒数：用于验证执行端在真实业务耗时下的吞吐与受理队列饱和行为
        var parameter = context.JobExecuteInfo?.Parameter;
        if (!string.IsNullOrWhiteSpace(parameter)
            && int.TryParse(parameter, out var delayMs)
            && delayMs > 0)
        {
            await Task.Delay(delayMs, cancellationToken);
        }

        return JobResult.Success;
    }
}
