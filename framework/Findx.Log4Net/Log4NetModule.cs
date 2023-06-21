using System;
using System.ComponentModel;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Findx.Log4Net
{
    [Description("Findx-Log4Net组件模块")]
    public class Log4NetModule : StartupModule
    {
        public override ModuleLevel Level => ModuleLevel.Framework;
        public override int Order => 50;

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILoggerProvider, Log4NetLoggerProvider>();

            return services;
        }

        public override void UseModule(IServiceProvider provider)
        {
            base.UseModule(provider);
        }
    }
}