using System.ComponentModel;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Findx.NLog
{
    /// <summary>
    ///     Findx-NLog组件模块
    /// </summary>
    [Description("Findx-NLog组件模块")]
    public class NLogModule : FindxModule
    {
        /// <summary>
        ///     模块等级
        /// </summary>
        public override ModuleLevel Level => ModuleLevel.Application;

        /// <summary>
        ///     模块排序
        /// </summary>
        public override int Order => 50;

        /// <summary>
        ///     配置模块服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILoggerProvider, NLogLoggerProvider>();

            return services;
        }
    }
}