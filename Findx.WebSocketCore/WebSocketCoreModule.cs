using Findx.Extensions;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Reflection;

namespace Findx.WebSocketCore
{
    [Description("Findx-WebSocket模块")]
    public class WebSocketCoreModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Application;
        public override int Order => 20;

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<WebSocketConnectionManager>();

            IWebSocketHandlerTypeFinder handlerTypeFinder = services.GetOrAddTypeFinder<IWebSocketHandlerTypeFinder>(assemblyFinder => new WebSocketHandlerTypeFinder(assemblyFinder));
            Type[] moduleTypes = handlerTypeFinder.FindAll();
            foreach (var type in moduleTypes)
            {
                if (type.GetTypeInfo().BaseType == typeof(WebSocketHandler))
                {
                    services.AddSingleton(type);
                }
            }
            return services;
        }
    }
}
