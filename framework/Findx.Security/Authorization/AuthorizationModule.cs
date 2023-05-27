using System.ComponentModel;
using Findx.AspNetCore;
using Findx.Extensions;
using Findx.Modularity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.Security.Authorization;

/// <summary>
///     Findx-授权模块
/// </summary>
[Description("Findx-授权模块")]
public class AuthorizationModule : AspNetCoreModuleBase
{
    private bool _enabled;

    /// <summary>
    ///     等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Application;

    /// <summary>
    ///     排序
    /// </summary>
    public override int Order => 15;

    /// <summary>
    ///     配置服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        var configuration = services.GetConfiguration();
        var section = configuration.GetSection("Findx:Authorization");
        services.Configure<AuthorizationOptions>(section);

        _enabled = configuration.GetValue<bool>("Findx:Authorization:Enabled");
        if (!_enabled) return services;
        services.AddAuthorization(opts =>
        {
            opts.AddPolicy(FunctionRequirement.Policy, policy => policy.AddRequirements(new FunctionRequirement()));
        });
        services.AddScoped<IAuthorizationHandler, FunctionRequirementHandler>();
        return services;
    }

    /// <summary>
    ///     启用模块
    /// </summary>
    /// <param name="app"></param>
    public override void UseModule(IApplicationBuilder app)
    {
        if (!_enabled) return;
        app.UseAuthorization();
        base.UseModule(app);
    }
}