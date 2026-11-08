using Findx.Extensions;
using Findx.Extensions.Scheduler.Scheduling;
using Findx.Extensions.Scheduler.Storage;
using Findx.Jobs;
using Findx.Jobs.Client;
using Findx.Jobs.Common;
using Findx.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.Extensions.Scheduler;

/// <summary>
///     Findx-分布式任务调度组件模块
/// </summary>
[Description("Findx-分布式任务调度组件")]
[DependsOnModules([typeof(JobModule)])]
public class SchedulerModule : StartupModule
{
    /// <summary>
    ///     模块等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Application;

    /// <summary>
    ///     模块排序
    /// </summary>
    public override int Order => 120;

    /// <summary>
    ///     配置服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();

        var configuration = services.GetConfiguration();
        services.Configure<SchedulerOptions>(configuration.GetSection("Findx:Scheduler"));

        var options = new SchedulerOptions();
        configuration.GetSection("Findx:Scheduler").Bind(options);

        // 安全硬约束：启用调度端或执行端必须配置 ClusterToken。
        // 否则 execute/result 接口默认零鉴权——内网任意客户端可触发任意 IJob 执行（RCE 面）或伪造结果回报污染任务状态
        if ((options.SchedulerEnabled || options.WorkerEnabled) && string.IsNullOrWhiteSpace(options.ClusterToken))
        {
            throw new InvalidOperationException(
                "[Findx:Scheduler] 已启用调度端/执行端但未配置 ClusterToken（机器间接口鉴权，请求须携带 X-Scheduler-Token 头）。" +
                "为防止内网任意触发 IJob 执行与伪造结果回报，请配置 Findx:Scheduler:ClusterToken。");
        }

        // 分发策略（默认 RoundRobin，含 Weight 扩展位）
        services.AddSingleton<IDispatchStrategy, RoundRobinDispatchStrategy>();

        // Master 侧核心服务（单例实例与 HostedService 必须为同一对象，否则接口注入方与 hosted 实例各持一份状态）
        services.AddSingleton<ScheduledJobStorage>();
        services.AddSingleton<InFlightExecutionRegistry>();
        services.AddSingleton<JobDispatcher>();
        services.AddSingleton<JobResultHandler>();
        services.AddSingleton<WorkerRegistry>();
        services.AddSingleton<MasterElectionService>();
        services.AddSingleton<DistributedScheduler>();

        services.AddHostedService(sp => sp.GetRequiredService<WorkerRegistry>());
        services.AddHostedService(sp => sp.GetRequiredService<MasterElectionService>());
        services.AddHostedService(sp => sp.GetRequiredService<DistributedScheduler>());

        // IJob 类型查找器（单例，Worker 能力扫描与执行注册共用）
        services.GetOrAddSingletonInstance<IJobFinder>(() => new JobFinder());

        // Worker 端：Distributed 模式下 JobModule 不注册任何执行服务，这里自行注册最小执行服务集
        if (options.WorkerEnabled)
        {
            var dict = new JobWorkSet();
            var jobFinder = services.GetOrAddSingletonInstance<IJobFinder>(() => new JobFinder());
            var jobTypes = jobFinder.FindAll(true);
            foreach (var jobType in jobTypes)
            {
                services.AddTransient(jobType);
                if (jobType.FullName != null) dict.TryAdd(jobType.FullName, jobType);
            }

            services.AddSingleton(dict);
            services.AddScoped<IJobExecutor, JobExecutor>();
        }

        return base.ConfigureServices(services);
    }
}
