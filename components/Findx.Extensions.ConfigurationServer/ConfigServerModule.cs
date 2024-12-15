using System.ComponentModel;
using Findx.Extensions.ConfigurationServer.Client;
using Findx.Extensions.ConfigurationServer.Handling;
using Findx.Extensions.ConfigurationServer.Services;
using Findx.Messaging;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.Extensions.ConfigurationServer;

/// <summary>
///     配置服务模块
/// </summary>
[Description("Findx-配置中心服务端")]
public class ConfigServerModule : StartupModule
{
    /// <summary>
    ///     等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Application;

    /// <summary>
    ///     排序
    /// </summary>
    public override int Order => 110;

    /// <summary>
    ///     配置服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();

        services.AddSingleton<IClientCallBack, ClientCallBack>();
        services.AddSingleton<IClusterService, ClusterService>();
        services.AddSingleton<IDumpService, DumpService>();
        services.AddScoped<IApplicationEventHandler<ConfigDataChangeEvent>, ConfigDataChangeEventHandler>();

        var configuration = services.GetConfiguration();

        // 应用基础
        services.Configure<ConfigServerOptions>(configuration.GetSection("Findx:Config"));

        return base.ConfigureServices(services);
    }

    /// <summary>
    ///     销毁回调
    /// </summary>
    /// <param name="provider"></param>
    public override void OnShutdown(IServiceProvider provider)
    {
        var clientCallBack = provider.GetService<IClientCallBack>();
        clientCallBack?.Dispose();
        base.OnShutdown(provider);
    }
}