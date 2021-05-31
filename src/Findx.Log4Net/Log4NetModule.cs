using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;

namespace Findx.Log4Net
{
    [Description("Findx-Log4Net组件模块")]
    public class Log4NetModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Application;
        public override int Order => 20;

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
