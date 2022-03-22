using Findx.AspNetCore;
using Findx.Extensions;
using Findx.Modularity;
using Findx.Security.Authorization.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Findx.Security.Authorization
{
    [Description("Findex-授权模块")]
    public class AuthorizationModule : AspNetCoreModuleBase
    {
        private bool Enabled;
        public override ModuleLevel Level => ModuleLevel.Application;
        public override int Order => 15;
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            IConfiguration configuration = services.GetConfiguration();
            var section = configuration.GetSection("Findx:Authorization");
            services.Configure<AuthorizationOptions>(section);

            Enabled = configuration.GetValue<bool>("Findx:Authorization:Enabled");
            if (Enabled)
            {
                services.AddSingleton<IPermissionChecker, PermissionChecker>();
                services.AddSingleton<IPermissionHandler, MvcPermissionHandler>();
                services.AddSingleton<IPermissionStore, MemoryPermissionStore>();

                services.AddAuthorization(opts =>
                {
                    opts.AddPolicy(PermissionRequirement.Policy, policy => policy.Requirements.Add(new PermissionRequirement()));
                });
                services.AddSingleton<IAuthorizationHandler, PermissionRequirementHandler>();
            }
            return services;
        }
        public override void UseModule(IApplicationBuilder app)
        {
            if (Enabled)
            {
                app.UseAuthorization();

                Task.Run(async () =>
                {
                    IServiceProvider provider = app.ApplicationServices;
                    IPermissionHandler permissionHandler = provider.GetService<IPermissionHandler>();
                    await permissionHandler?.InitializeAsync();
                });
                base.UseModule(app);
            }
        }
    }
}
