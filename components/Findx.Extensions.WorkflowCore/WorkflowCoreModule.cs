using System.ComponentModel;
using Findx.Extensions;
using Findx.Extensions.WorkflowCore.Engine;
using Findx.Extensions.WorkflowCore.Options;
using Findx.Extensions.WorkflowCore.Services;
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

        services.AddScoped<FlwStore>();
        services.AddScoped<IFlowLongEngine, FlowLongEngine>();
        services.AddScoped<FlwTimeoutHandler>();

        //  办理人解析扩展点：宿主可注册自己的 IFlwActorProvider（如查组织架构取上级），未注册时使用默认实现
        services.TryAddScoped<IFlwActorProvider, DefaultFlwActorProvider>();

        //  任务通知扩展点：提醒走宿主自己的消息渠道，未注册时只写日志
        services.TryAddScoped<IFlwTaskNotifier, DefaultFlwTaskNotifier>();

        return base.ConfigureServices(services);
    }
}
