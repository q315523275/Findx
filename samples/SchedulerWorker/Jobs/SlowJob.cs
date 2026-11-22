using System.Text.Json;
using Findx.Jobs.Client;
using Findx.Jobs.Common;

namespace Findx.Samples.SchedulerWorker.Jobs;

/// <summary>
///     演示任务三：长时间执行。
///     <para>用途是验证人工终止（管理面 POST executions/kill）与超时兜底：任务每 5 秒记一条进度，
///     收到取消信号后按协作式取消返回 Cancelled。</para>
/// </summary>
[Job(Name = "演示-长任务", Description = "默认执行 40 秒，用于演示 kill 与超时")]
public class SlowJob : IJob
{
    public async Task<JobResult> RunAsync(JobExecuteContext context, CancellationToken cancellationToken = default)
    {
        var seconds = ReadSeconds(context.JobExecuteInfo.Parameter);
        var log = context.LogRecorder;
        log.Info($"[SlowJob] 开始执行，预计耗时 {seconds} 秒；可在管理面调用 executions/kill 终止");

        for (var i = 1; i <= seconds; i++)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                //  被 kill 或宿主停机：把中断点写进执行日志，便于事后判断是人工终止还是超时
                log.Warn($"[SlowJob] 在第 {i} 秒收到取消信号，提前结束");
                return JobResult.CancelledWithMessage($"任务在第 {i} 秒被取消");
            }

            if (i % 5 == 0 || i == seconds) log.Info($"[SlowJob] 进度 {i}/{seconds}");
        }

        log.Info("[SlowJob] 正常执行完毕");
        return JobResult.Success;
    }

    /// <summary>
    ///     从任务参数读取 seconds（缺省 40，最大 300，避免示例把调度队列占死）
    /// </summary>
    private static int ReadSeconds(string parameter)
    {
        if (string.IsNullOrWhiteSpace(parameter)) return 40;

        try
        {
            using var doc = JsonDocument.Parse(parameter);
            if (doc.RootElement.TryGetProperty("seconds", out var value) && value.TryGetInt32(out var seconds))
                return Math.Clamp(seconds, 1, 300);
        }
        catch (JsonException)
        {
            //  参数不是合法 JSON 时退回默认值：示例不因为参数写错就跑不起来
        }

        return 40;
    }
}
