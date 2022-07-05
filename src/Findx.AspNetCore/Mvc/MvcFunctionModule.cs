using Findx.AspNetCore;
using Findx.Extensions;
using Findx.Modularity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Threading.Tasks;
using Findx.Security;

namespace Findx.AspNetCore.Mvc
{
    [Description("Findex-Mvc功能信息模块")]
    public class AuthorizationModule : AspNetCoreModuleBase
    {
        public override ModuleLevel Level => ModuleLevel.Application;
        public override int Order => 14;
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFunctionHandler, MvcFunctionHandler>();
            services.AddSingleton<IFunctionStore<MvcFunction>, MvcFunctionStore>();
            
            return services;
        }
        public override void UseModule(IApplicationBuilder app)
        {
            Task.Run(() =>
            {
                app.ApplicationServices.GetRequiredService<IFunctionHandler>().Initialize();
            });
        }
    }
}
