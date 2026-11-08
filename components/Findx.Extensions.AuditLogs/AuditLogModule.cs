using System.ComponentModel;
using Findx.Data;
using Findx.Extensions.AuditLogs.EventHandling;
using Findx.Extensions.AuditLogs.Events;
using Findx.Extensions.AuditLogs.Options;
using Findx.Extensions.AuditLogs.ServiceDefaults;
using Findx.Extensions.AuditLogs.Services;
using Findx.Messaging;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.Extensions.AuditLogs;

/// <summary>
///     Findx-审计日志模块
/// </summary>
[Description("Findx-审计日志模块")]
public class AuditLogModule: StartupModule
{
    /// <summary>
    ///     等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Application;

    /// <summary>
    ///     排序
    /// </summary>
    public override int Order => 120;
    
    
    /// <summary>
    ///     配置服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        //  GxP 完整性配置（Findx:AuditLog:EnableIntegrity）
        var configuration = services.GetConfiguration();
        services.Configure<AuditLogOptions>(configuration.GetSection("Findx:AuditLog"));

        services.AddScoped<IAuditStore, AuditStoreServices>();
        services.AddScoped<IApplicationEventHandler<AuditLogSaveEvent>, AuditLogSaveEventHandler>();
        services.AddScoped<AuditLogIntegrityValidator>();

        return base.ConfigureServices(services);
    }
}