using Findx.Extensions;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Reflection;

namespace Findx.WebSocketCore
{
    /// <summary>
    /// WebSocket模块
    /// </summary>
    [Description("Findx-WebSocket模块")]
    public class WebSocketCoreModule : FindxModule
    {
        /// <summary>
        /// 等级
        /// </summary>
        public override ModuleLevel Level => ModuleLevel.Application;
        
        /// <summary>
        /// 排序
        /// </summary>
        public override int Order => 20;

        /// <summary>
        /// 配置服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<WebSocketConnectionManager>();
            services.AddSingleton<IWebSocketSerializer, WebSocketSerializer>();
            
            var handlerTypeFinder = services.GetOrAddTypeFinder<IWebSocketHandlerTypeFinder>(assemblyFinder => new WebSocketHandlerTypeFinder(assemblyFinder));
            var moduleTypes = handlerTypeFinder.FindAll();
            foreach (var type in moduleTypes)
            {
                if (type.GetTypeInfo().BaseType == typeof(WebSocketHandlerBase))
                {
                    services.AddSingleton(type);
                }
            }
            return services;
        }
    }
}
