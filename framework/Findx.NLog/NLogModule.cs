using System.ComponentModel;
using Findx.Extensions;
using Findx.Modularity;
using Findx.NLog.LayoutRenderX;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NLog;

namespace Findx.NLog;

/// <summary>
///     Findx-NLog组件模块
/// </summary>
[Description("Findx-NLog组件模块")]
public class NLogModule : StartupModule
{
    /// <summary>
    ///     模块等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Framework;

    /// <summary>
    ///     模块排序
    /// </summary>
    public override int Order => 90;

    /// <summary>
    ///     配置模块服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // 配置服务
        var configuration = services.GetConfiguration();
        if (!configuration.GetValue<bool>("Logging:Enabled")) return services;
        
        services.Replace(new ServiceDescriptor(typeof(ILoggerProvider), typeof(NLogLoggerProvider), ServiceLifetime.Singleton));
        //  LayoutRenderer.Register<TraceIdentifierLayoutRenderer>("TraceIdentifier");
        LogManager.Setup().SetupExtensions(builder =>
        {
            builder.RegisterLayoutRenderer<TraceIdentifierLayoutRenderer>("TraceIdentifier");
        });
        
        return services;
    }
}