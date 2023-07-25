using System;
using System.ComponentModel;
using Findx.Component.DistributedConfigurationCenter.Client;
using Findx.Component.DistributedConfigurationCenter.Handling;
using Findx.Component.DistributedConfigurationCenter.Services;
using Findx.Extensions;
using Findx.Messaging;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.Component.DistributedConfigurationCenter;

/// <summary>
///     配置服务模块
/// </summary>
[Description("Findx配置中心-服务端模块")]
public class ConfigServiceModule : StartupModule
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
        services.Configure<ConfigServiceOptions>(configuration.GetSection("Findx:Config"));

        return base.ConfigureServices(services);
    }

    /// <summary>
    ///     销毁回调
    /// </summary>
    /// <param name="provider"></param>
    public override void OnShutdown(IServiceProvider provider)
    {
        var clientCallBack = provider.GetRequiredService<IClientCallBack>();
        clientCallBack.Dispose();
        base.OnShutdown(provider);
    }
}