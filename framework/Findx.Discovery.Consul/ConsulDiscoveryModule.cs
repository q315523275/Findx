using System;
using System.ComponentModel;
using Findx.Discovery.Abstractions;
using Findx.Extensions;
using Findx.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Discovery.Consul;

/// <summary>
///     Findx-Consul服务发现模块
/// </summary>
[Description("Findx-Consul服务发现模块")]
[DependsOnModules([typeof(DiscoveryCoreModule)])]
public class ConsulDiscoveryModule : StartupModule
{
    /// <summary>
    ///     模块等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Framework;

    /// <summary>
    ///     模块排序
    /// </summary>
    public override int Order => 100;

    /// <summary>
    ///     模块配置服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        var configuration = services.GetConfiguration();
        if (!configuration.GetValue<bool>("Findx:Discovery:Enabled")) 
            return services;

        var section = configuration.GetSection("Findx:Consul");
        services.Configure<ConsulOptions>(section);

        services.TryAddSingleton<IConsulServiceRegistry, ConsulServiceRegistry>();
        services.TryAddSingleton<IConsulRegistration, ConsulRegistration>();
        services.TryAddSingleton<IServiceEndPointProvider, ConsulServiceEndPointProvider>();
        services.TryAddSingleton(sp => ConsulClientFactory.CreateClient(sp.GetRequiredService<IOptionsMonitor<ConsulOptions>>().CurrentValue));

        // 自动注册服务发现
        services.AddHostedService<ConsulDiscoveryAutoRegistryWorker>();

        return services;
    }
        
    /// <summary>
    /// 启用模块
    /// </summary>
    /// <param name="app"></param>
    public override void UseModule(IServiceProvider app)
    {
        var optionsMonitor = app.GetRequiredService<IOptionsMonitor<DiscoveryOptions>>();
        if (optionsMonitor.CurrentValue.Enabled) base.UseModule(app);
    }

    /// <summary>
    ///     模块销毁
    /// </summary>
    /// <param name="provider"></param>
    public override void OnShutdown(IServiceProvider provider)
    {
        var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<DiscoveryOptions>>();
        if (optionsMonitor.CurrentValue.Enabled && optionsMonitor.CurrentValue.Deregister)
        {
            try
            {
                var consulServiceRegistry = provider.GetRequiredService<IConsulServiceRegistry>();
                var consulRegistration = provider.GetRequiredService<IConsulRegistration>();

                //  阻塞等待注销请求真正发出；原写法只取 awaiter 未 GetResult，进程退出时 HTTP 请求尚未完成
                consulServiceRegistry.DeregisterAsync(consulRegistration).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                try
                {
                    provider.GetService<ILogger<ConsulDiscoveryModule>>()?.LogWarning(ex, "Consul 服务注销失败，将等待健康检查 CriticalTimeout 后由 Consul 端摘除");
                }
                catch
                {
                    // 关闭阶段日志失败不再向外抛，不阻断 base.OnShutdown
                }
            }
        }
        base.OnShutdown(provider);
    }
}