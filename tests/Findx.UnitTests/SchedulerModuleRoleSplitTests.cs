using Findx.Extensions.Scheduler;
using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Execution;
using Findx.Extensions.Scheduler.Scheduling;
using Findx.Extensions.Scheduler.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.UnitTests;

/// <summary>
///     SchedulerModule 角色隔离回归测试。
///     约束：执行端（Worker）不持有任何数据库能力，登记 / 结果 / 日志全部经协议通道上送；
///     调度端（Master）才注册数据库原语与账本。破坏这条约束会让 Worker 需要数据库连接（部署与安全风险）。
/// </summary>
public class SchedulerModuleRoleSplitTests
{
    private static ServiceCollection BuildServices(bool schedulerEnabled, bool workerEnabled)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["Findx:Scheduler:SchedulerEnabled"] = schedulerEnabled.ToString(),
            ["Findx:Scheduler:WorkerEnabled"] = workerEnabled.ToString(),
            ["Findx:Scheduler:ClusterToken"] = "test-token",
            ["Findx:Scheduler:NodeAddress"] = "http://worker:5000",
            ["Findx:Jobs:Enabled"] = "true",
            ["Findx:Jobs:ScheduleType"] = "Distributed"
        }).Build();
        services.AddSingleton<IConfiguration>(configuration);
        return services;
    }

    private static bool Has(IServiceCollection services, Type type)
    {
        return services.Any(d => d.ServiceType == type);
    }

    [Fact]
    public void WorkerOnly_DoesNotRegisterAnyDatabasePrimitive()
    {
        var services = BuildServices(schedulerEnabled: false, workerEnabled: true);

        new SchedulerModule().ConfigureServices(services);

        //  数据库原语与账本一律不得出现在执行端容器里
        Assert.False(Has(services, typeof(IWorkerDirectory)));
        Assert.False(Has(services, typeof(IExecutionLedger)));
        Assert.False(Has(services, typeof(IExecutionLogStore)));
        Assert.False(Has(services, typeof(IMasterLease)));
        Assert.False(Has(services, typeof(IPeriodClaimer)));
        Assert.False(Has(services, typeof(ScheduledJobStorage)));
        //  调度端专用服务同样不注册（避免执行端被误判为可调度/可接结果）
        Assert.False(Has(services, typeof(WorkerRegistry)));
        Assert.False(Has(services, typeof(MasterElectionService)));
        Assert.False(Has(services, typeof(JobDispatcher)));
        Assert.False(Has(services, typeof(JobResultHandler)));
    }

    [Fact]
    public void WorkerOnly_RegistersProtocolChannelAndHeartbeat()
    {
        var services = BuildServices(schedulerEnabled: false, workerEnabled: true);

        new SchedulerModule().ConfigureServices(services);

        Assert.True(Has(services, typeof(ISchedulerMasterChannel)));
        Assert.True(Has(services, typeof(WorkerHeartbeatService)));
        Assert.True(Has(services, typeof(JobExecutionQueue)));
    }

    [Fact]
    public void MasterOnly_RegistersDatabasePrimitivesButNoWorkerExecution()
    {
        var services = BuildServices(schedulerEnabled: true, workerEnabled: false);

        new SchedulerModule().ConfigureServices(services);

        Assert.True(Has(services, typeof(IExecutionLedger)));
        Assert.True(Has(services, typeof(IWorkerDirectory)));
        Assert.True(Has(services, typeof(IMasterLease)));
        Assert.True(Has(services, typeof(WorkerRegistry)));
        Assert.True(Has(services, typeof(JobResultHandler)));

        //  纯调度节点不注册执行能力与上行通道：受理了也没人执行，必须显式 503
        Assert.False(Has(services, typeof(JobExecutionQueue)));
        Assert.False(Has(services, typeof(WorkerHeartbeatService)));
        Assert.False(Has(services, typeof(ISchedulerMasterChannel)));
    }

    [Fact]
    public void BothRoles_RegisterBothSets()
    {
        var services = BuildServices(schedulerEnabled: true, workerEnabled: true);

        new SchedulerModule().ConfigureServices(services);

        Assert.Contains(services, d => d.ServiceType == typeof(ISchedulerMasterChannel));
        Assert.Contains(services, d => d.ServiceType == typeof(IExecutionLedger));
    }
}
