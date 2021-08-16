using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;

namespace Findx.NLog
{
    [Description("Findx-NLog组件模块")]
    public class NLogModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Framework;
        public override int Order => 50;

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILoggerProvider, NLogLoggerProvider>();

            return services;
        }

        public override void UseModule(IServiceProvider provider)
        {

            base.UseModule(provider);
        }
    }
}
