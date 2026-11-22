using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Findx.Extensions.Scheduler.Storage;
using Findx.Jobs.Common;
using Microsoft.Extensions.Options;

namespace Findx.Samples.SchedulerServer;

/// <summary>
///     演示任务定义播种器。
///     <para>任务定义是「数据」不是「代码」：调度端只认 scheduler_jobs 里的 JobName + HandlerType + Cron，
///     作业实现永远留在执行端。所以服务端不需要引用 Worker 项目，只需要知道 HandlerType 的字符串。</para>
///     <para>生产环境不要用代码播种，应该走管理面 POST /api/scheduler/admin/jobs/save。</para>
/// </summary>
public static class DemoJobSeeder
{
    private record SeedJob(string JobName, string HandlerType, string Cron, string Parameter, string Remark);

    private static readonly SeedJob[] Seeds =
    [
        new("demo-hello", "Findx.Samples.SchedulerWorker.Jobs.HelloJob", "*/10 * * * * ?", null,
            "每 10 秒成功一次：验证 分发 → 执行 → 回调 → 账本 闭环"),
        new("demo-flaky", "Findx.Samples.SchedulerWorker.Jobs.FlakyJob", "*/30 * * * * ?", null,
            "每 30 秒失败一次：验证 退避重试 → 死信隔离 → 人工放行"),
        new("demo-slow", "Findx.Samples.SchedulerWorker.Jobs.SlowJob", "0 */5 * * * ?", "{\"seconds\":40}",
            "每 5 分钟执行 40 秒：验证 人工终止 / 超时兜底")
    ];

    /// <summary>
    ///     播种示例任务（仅调度端、默认开启，可用 Findx:Samples:SeedDemoJobs=false 关闭）
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        if (!configuration.GetValue("Findx:Samples:SeedDemoJobs", true)) return;

        var scheduler = services.GetRequiredService<IOptions<SchedulerOptions>>().Value;
        if (!scheduler.SchedulerEnabled) return;

        try
        {
            var storage = services.GetRequiredService<ScheduledJobStorage>();
            var existing = await storage.GetListAsync();
            var existingNames = existing.Select(x => x.JobName).ToHashSet(StringComparer.Ordinal);

            foreach (var seed in Seeds)
            {
                if (existingNames.Contains(seed.JobName))
                {
                    logger.LogInformation("[DemoSeed] 任务 {JobName} 已存在，跳过", seed.JobName);
                    continue;
                }

                var job = new ScheduledJob
                {
                    JobName = seed.JobName,
                    HandlerType = seed.HandlerType,
                    CronExpression = seed.Cron,
                    Parameter = seed.Parameter,
                    Status = JobStatus.Normal,
                    Remark = seed.Remark,
                    TryCount = 0
                };

                var added = await storage.AddAsync(job);
                logger.LogInformation("[DemoSeed] 已创建任务 {JobName}（{HandlerType}，Cron={Cron}，首次触发 {NextRunTime}）",
                    added.JobName, added.HandlerType, added.CronExpression, added.NextRunTime);
            }
        }
        catch (Exception ex)
        {
            //  库没准备好时不要让示例直接起不来：把原因打出来，接口仍然可访问（管理面会报具体错误）
            logger.LogError(ex, "[DemoSeed] 播种示例任务失败，请确认 MySQL 可访问且已执行 docs/sql/scheduler_v2.sql");
        }
    }
}
