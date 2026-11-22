using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Execution;
using Findx.Extensions.Scheduler.Options;
using Findx.Extensions.Scheduler.Scheduling;
using Findx.Extensions.Scheduler.Storage;
using Findx.Jobs;
using Findx.Jobs.Client;
using Findx.Jobs.Common;
using Findx.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Findx.Extensions.Scheduler;

/// <summary>
///     Findx-分布式任务调度组件模块。
///     <para>基础版本只依赖数据库与 HTTP：主节点租约、节点心跳、执行账本全部落库，作业实体通过 HTTP 投递。</para>
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

        //  安全硬约束：启用调度端或执行端必须配置 ClusterToken。
        //  否则 execute/result/beat/kill 接口默认零鉴权——内网任意客户端可触发任意 IJob 执行（RCE 面）或伪造结果回报污染任务状态
        if ((options.SchedulerEnabled || options.WorkerEnabled) && string.IsNullOrWhiteSpace(options.ClusterToken))
        {
            throw new InvalidOperationException(
                "[Findx:Scheduler] 已启用调度端/执行端但未配置 ClusterToken（机器间接口鉴权，请求须携带 X-Scheduler-Token 头）。" +
                "为防止内网任意触发 IJob 执行与伪造结果回报，请配置 Findx:Scheduler:ClusterToken。");
        }

        //  内核互斥：分布式组件与内存版 JobModule 不能同时点火，否则同一 IJob 会被两套循环各触发一次
        var jobsEnabled = configuration.GetSection("Findx:Jobs").GetValue<bool>("Enabled");
        var scheduleType = configuration.GetSection("Findx:Jobs").GetValue("ScheduleType", ScheduleType.Simple);
        if ((options.SchedulerEnabled || options.WorkerEnabled) && !(jobsEnabled && scheduleType == ScheduleType.Distributed))
        {
            throw new InvalidOperationException(
                "[Findx:Scheduler] 启用分布式调度时必须同时配置 Findx:Jobs:Enabled=true 且 Findx:Jobs:ScheduleType=Distributed，" +
                "否则内存版 JobModule 与分布式组件会同时运行，同一作业被重复触发。");
        }

        //  IJob 类型查找器：内存扫描，两个角色都用，不涉及数据库
        services.GetOrAddSingletonInstance<IJobFinder>(() => new JobFinder());

        //  时钟：默认本地实现（纯内存），仅调度端会用到持久化能力；需要多节点统一时间时替换实现即可
        services.AddSingleton<IClusterClock, LocalClusterClock>();

        //  ==================== 执行端（Worker）：零数据库依赖，只用协议 ====================
        //  硬约束：执行端进程不注册任何 IRepository / 数据库原语。节点登记、结果回报、日志上报全部经
        //  ISchedulerMasterChannel 出站（当前 HTTP，后续可替换 gRPC/RPC，执行端其余代码不感知）。
        if (options.WorkerEnabled)
        {
            //  上行协议通道：唯一的出站出口。换 RPC 只需替换本行注册，Worker 内部无需改动
            services.AddSingleton<ISchedulerMasterChannel, HttpSchedulerMasterChannel>();

            //  执行过程日志：旁路采集 + 分片批量上报（上报走协议通道，不直接写库）
            services.AddSingleton<ExecutionLogCollector>();

            //  结果上报队列：执行结束的结果先入队，后台按窗口合并成批上报（1 = 关闭批量，回落逐条）
            services.AddSingleton<JobResultShipper>();
            services.AddHostedService(sp => sp.GetRequiredService<JobResultShipper>());

            services.AddSingleton<JobResultReporter>();
            services.AddSingleton<JobExecutionQueue>();

            //  Worker 端最小执行服务集（Distributed 模式下 JobModule 不注册执行服务）
            var dict = new JobWorkSet();
            var jobFinder = services.GetOrAddSingletonInstance<IJobFinder>(() => new JobFinder());
            foreach (var jobType in jobFinder.FindAll(true))
            {
                services.AddTransient(jobType);
                if (jobType.FullName != null) dict.TryAdd(jobType.FullName, jobType);
            }

            services.AddSingleton(dict);
            services.AddScoped<IJobExecutor, JobExecutor>();

            //  执行队列只在执行端点火：纯调度节点启动它会导致「受理成功但没人执行」
            services.AddHostedService(sp => sp.GetRequiredService<JobExecutionQueue>());

            //  日志旁路采集（不替换 NLog/控制台等既有提供器）
            services.AddSingleton<ILoggerProvider, ExecutionLogProvider>();

            //  日志分片上报队列
            services.AddSingleton<ExecutionLogShipper>();
            services.AddHostedService(sp => sp.GetRequiredService<ExecutionLogShipper>());

            //  心跳：扫描本地任务清单 + 协议上报；停机时下线。执行端不写库，登记由调度端接口落库
            services.AddSingleton<WorkerHeartbeatService>();
            services.AddHostedService(sp => sp.GetRequiredService<WorkerHeartbeatService>());
        }

        //  ==================== 调度端（Master）：数据库原语 + 账本 + 分发 ====================
        if (options.SchedulerEnabled)
        {
            //  分发策略（默认 RoundRobin）
            services.AddSingleton<IDispatchStrategy, RoundRobinDispatchStrategy>();

            //  协调原语：全部落在数据源 "jobs" 的几张表上，替换实现即可切回其它中间件
            services.AddSingleton<IMasterLease, DatabaseMasterLease>();
            services.AddSingleton<IWorkerDirectory, DatabaseWorkerDirectory>();
            services.AddSingleton<IExecutionLedger, DatabaseExecutionLedger>();
            services.AddSingleton<IPeriodClaimer, DatabasePeriodClaimer>();
            services.AddSingleton<IExecutionLogStore, DatabaseExecutionLogStore>();

            //  Master 侧核心服务（单例实例与 HostedService 必须为同一对象，否则接口注入方与 hosted 实例各持一份状态）
            services.AddSingleton<ScheduledJobStorage>();
            services.AddSingleton<JobDispatcher>();
            services.AddSingleton<JobResultHandler>();
            services.AddSingleton<WorkerRegistry>();
            services.AddSingleton<MasterElectionService>();
            services.AddSingleton<SchedulerJobBuffer>();
            services.AddSingleton<SchedulerDispatchQueue>();
            services.AddSingleton<DistributedScheduler>();

            //  超时兜底独立节拍：批处理写库不能挤在到期扫描循环里
            services.AddSingleton<TimeoutSweepService>();

            services.AddHostedService(sp => sp.GetRequiredService<WorkerRegistry>());
            services.AddHostedService(sp => sp.GetRequiredService<MasterElectionService>());
            services.AddHostedService(sp => sp.GetRequiredService<SchedulerDispatchQueue>());
            services.AddHostedService(sp => sp.GetRequiredService<DistributedScheduler>());
            services.AddHostedService(sp => sp.GetRequiredService<TimeoutSweepService>());

            //  执行历史清理：只在 Master 上执行（账本 + 日志明细按 RetentionDays 一起清）
            services.AddSingleton<ExecutionRetentionService>();
            services.AddHostedService(sp => sp.GetRequiredService<ExecutionRetentionService>());
        }

        return base.ConfigureServices(services);
    }
}
