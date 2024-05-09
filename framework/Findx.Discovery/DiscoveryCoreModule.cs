using System;
using System.ComponentModel;
using Findx.Discovery.Abstractions;
using Findx.Discovery.HttpMessageHandlers;
using Findx.Discovery.Internals;
using Findx.Discovery.LoadBalancer;
using Findx.Extensions;
using Findx.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Findx.Discovery;

/// <summary>
///     服务发现模块核心类
/// </summary>
[Description("Findx-服务发现核心模块")]
public class DiscoveryCoreModule : StartupModule
{
    /// <summary>
    ///     获取 模块级别，级别越小越先启动
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Framework;

    /// <summary>
    ///     模块排序
    /// </summary>
    public override int Order => 95;

    /// <summary>
    /// 配置服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        var configuration = services.GetConfiguration();
        
        if (!configuration.GetValue<bool>("Findx:Discovery:Enabled")) return services;

        var section = configuration.GetSection("Findx:Discovery");
        services.Configure<DiscoveryOptions>(section);

        services.TryAddSingleton<ILoadBalancerFactory, LoadBalancerFactory>();
        services.TryAddSingleton<ILoadBalancerProvider, LoadBalancerProvider>();
        services.TryAddSingleton<IDiscoveryClient, DiscoveryClient>();

        services.TryAddTransient<DiscoveryLeastConnectHttpMessageHandler>();
        services.TryAddTransient<DiscoveryRandomHttpMessageHandler>();
        services.TryAddTransient<DiscoveryRoundRobinHttpMessageHandler>();
        services.TryAddTransient<DiscoveryIpHashHttpMessageHandler>();

        services.TryAddSingleton<IServiceEndPointProvider, ConfigServiceEndPointProvider>();
        services.TryAddSingleton<IServiceEndPointProviderFactory, ServiceEndPointProviderFactory>();
        
        return services;
    }
}