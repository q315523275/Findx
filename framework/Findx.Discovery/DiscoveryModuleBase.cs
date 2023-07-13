using System;
using Findx.Discovery.HttpMessageHandlers;
using Findx.Discovery.LoadBalancer;
using Findx.Extensions;
using Findx.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Findx.Discovery;

/// <summary>
/// 服务发现模块基类
/// </summary>
public abstract class DiscoveryModuleBase : StartupModule
{
    /// <summary>
    ///     获取 模块级别，级别越小越先启动
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Framework;

    /// <summary>
    /// 配置
    /// </summary>
    protected IConfiguration Configuration { get; set; }

    /// <summary>
    /// 配置服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        Configuration = services.GetConfiguration();
        
        if (!Configuration.GetValue<bool>("Findx:Discovery:Enabled")) return services;

        var section = Configuration.GetSection("Findx:Discovery");
        services.Configure<DiscoveryOptions>(section);

        services.TryAddSingleton<ILoadBalancerFactory, LoadBalancerFactory>();
        services.TryAddSingleton<ILoadBalancerProvider, LoadBalancerProvider>();
        services.TryAddSingleton<IDiscoveryClient, DiscoveryClient>();

        services.TryAddTransient<DiscoveryLeastConnectHttpMessageHandler>();
        services.TryAddTransient<DiscoveryRandomHttpMessageHandler>();
        services.TryAddTransient<DiscoveryRoundRobinHttpMessageHandler>();

        return services;
    }
}