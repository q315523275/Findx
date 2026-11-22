using System.ComponentModel;
using Findx.Data;
using Findx.Extensions;
using Findx.Extensions.WorkflowCore.Engine;
using Findx.Extensions.WorkflowCore.Entity;
using Findx.Extensions.WorkflowCore.Options;
using Findx.Extensions.WorkflowCore.ServiceDefaults;
using Findx.Extensions.WorkflowCore.Storage;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Findx.Extensions.WorkflowCore;

/// <summary>
///     Findx-工作流组件模块（FlowLong 风格的树形节点驱动审批流）
/// </summary>
[Description("Findx-工作流组件")]
public class WorkflowCoreModule: StartupModule
{
    /// <summary>
    ///     模块等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Application;

    /// <summary>
    ///     模块排序
    /// </summary>
    public override int Order => 130;

    /// <summary>
    ///     配置服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        var configuration = services.GetConfiguration();
        services.Configure<WorkflowCoreOptions>(configuration.GetSection("Findx:Workflow"));

        //  工作流表数据源：未配置时跟随主数据源；配置后写到实体扩展特性上
        var dataSource = configuration["Findx:Workflow:DataSource"];
        if (!string.IsNullOrWhiteSpace(dataSource))
            ApplyEntityDataSource(dataSource);

        services.AddScoped<FlwStore>();
        services.AddScoped<IFlowLongEngine, FlowLongEngine>();
        services.AddScoped<FlwTimeoutHandler>();

        //  办理人解析扩展点：宿主可注册自己的 IFlwActorProvider（如查组织架构取上级），未注册时使用默认实现
        services.TryAddScoped<IFlwActorProvider, DefaultFlwActorProvider>();

        //  任务通知扩展点：提醒走宿主自己的消息渠道，未注册时只写日志
        services.TryAddScoped<IFlwTaskNotifier, DefaultFlwTaskNotifier>();
        //  业务单据适配器注册表：宿主注册多个 IFlwBusinessAdapter 即可外挂多类业务单据
        services.TryAddScoped<FlwBusinessAdapterRegistry>();
        //  审批组件聚合服务
        services.AddScoped<FlwPanelService>();

        return base.ConfigureServices(services);
    }

    /// <summary>
    ///     8 张工作流表对应的实体
    /// </summary>
    private static readonly Type[] EntityTypes =
    [
        typeof(FlwProcessInfo), typeof(FlwInstanceInfo), typeof(FlwExtInstanceInfo), typeof(FlwTaskInfo),
        typeof(FlwTaskActorInfo), typeof(FlwHisInstanceInfo), typeof(FlwHisTaskInfo), typeof(FlwHisTaskActorInfo)
    ];

    /// <summary>
    ///     把配置的数据源写入实体扩展特性缓存（实体类上不再写死 DataSource，换库只改配置）
    /// </summary>
    /// <param name="dataSource"></param>
    private static void ApplyEntityDataSource(string dataSource)
    {
        foreach (var entityType in EntityTypes)
            entityType.GetEntityExtensionAttribute().DataSource = dataSource;
    }
}
