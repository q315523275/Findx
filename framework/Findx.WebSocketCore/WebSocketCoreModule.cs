using System;
using System.ComponentModel;
using System.Reflection;
using Findx.Extensions;
using Findx.Modularity;
using Findx.WebSocketCore.Abstractions;
using Findx.WebSocketCore.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.WebSocketCore;

/// <summary>
///     WebSocket模块
/// </summary>
[Description("Findx-WebSocket模块")]
public class WebSocketCoreModule : StartupModule
{
    /// <summary>
    ///     等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Application;

    /// <summary>
    ///     排序
    /// </summary>
    public override int Order => 60;

    /// <summary>
    ///     配置服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IWebSocketSessionManager, WebSocketSessionManager>();
        services.AddSingleton<IWebSocketSerializer, WebSocketSerializer>();
        services.AddSingleton<IWebSocketAuthorization, DefaultWebSocketAuthorization>();

        var handlerTypeFinder = services.GetOrAddTypeFinder<IWebSocketHandlerTypeFinder>(assemblyFinder => new WebSocketHandlerTypeFinder(assemblyFinder));
        var moduleTypes = handlerTypeFinder.FindAll();
        foreach (var type in moduleTypes)
            if (type.GetTypeInfo().BaseType == typeof(WebSocketHandlerBase)) services.AddSingleton(type);
        
        return services;
    }

    /// <summary>
    ///     应用关闭
    /// </summary>
    /// <param name="provider"></param>
    public override void OnShutdown(IServiceProvider provider)
    {
        var sessionManager = provider.GetService<IWebSocketSessionManager>();
        sessionManager.DisposeAsync();
    }
}