using System;
using Findx.Modularity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Threading.Tasks;
using Findx.Security;

namespace Findx.AspNetCore.Mvc
{
    /// <summary>
    /// Findx-Mvc功能信息模块
    /// </summary>
    [Description("Findx-Mvc功能信息模块")]
    public class AuthorizationModule : AspNetCoreModuleBase
    {
        /// <summary>
        /// 等级
        /// </summary>
        public override ModuleLevel Level => ModuleLevel.Application;
        
        /// <summary>
        /// 排序
        /// </summary>
        public override int Order => 14;
        
        /// <summary>
        /// 配置服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFunctionHandler, MvcFunctionHandler>();
            services.AddSingleton<IFunctionStore<MvcFunction>, MvcFunctionStore>();
            
            return services;
        }
        
        /// <summary>
        /// 启用模块
        /// </summary>
        /// <param name="app"></param>
        public override void UseModule(IApplicationBuilder app)
        {
            Task.Run(() =>
            {
                app.ApplicationServices.GetRequiredService<IFunctionHandler>().Initialize();
            });
            base.UseModule(app);
        }
    }
}
