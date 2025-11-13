using System;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Findx.AspNetCore;
using Findx.Common;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Modularity;
using Findx.Security.Authentication.Cookie;
using Findx.Security.Authentication.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Findx.Security.Authentication;

/// <summary>
///     Findx-认证模块
/// </summary>
[Description("Findx-认证模块")]
public sealed class AuthenticationModule : WebApplicationModuleBase
{
    /// <summary>
    ///     是否启用
    /// </summary>
    private bool _enabled;

    /// <summary>
    ///     等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Application;

    /// <summary>
    ///     排序
    /// </summary>
    public override int Order => 20;

    /// <summary>
    ///     添加服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        var configuration = services.GetConfiguration();
        _enabled = configuration.GetValue<bool>("Findx:Authentication:Enabled");
        if (!_enabled) return services;

        services.AddScoped<FindxCookieAuthenticationEvents>();
        services.AddScoped<FindxJwtBearerEvents>();
        services.AddScoped<IJwtTokenBuilder, DefaultJwtTokenBuilder>();
        services.AddSingleton<IAdvancedTokenValidator, DefaultAdvancedTokenValidator>();

        var defaultScheme = configuration.GetValue<string>("Findx:Authentication:DefaultScheme");

        var builder = services.AddAuthentication(defaultScheme ?? JwtBearerDefaults.AuthenticationScheme);

        AddJwtBearer(services, builder);
        AddCookie(services, builder);

        return services;
    }

    /// <summary>
    ///     使用模块
    /// </summary>
    /// <param name="app"></param>
    public override void UseModule(WebApplication app)
    {
        if (!_enabled) return;

        app.UseAuthentication().UseMiddleware<JwtTokenRenewalMiddleware>();

        base.UseModule(app);
    }

    /// <summary>
    ///     添加Jwt认证
    /// </summary>
    /// <param name="services"></param>
    /// <param name="builder"></param>
    /// <returns></returns>
    private static void AddJwtBearer(IServiceCollection services, AuthenticationBuilder builder)
    {
        var configuration = services.GetConfiguration();
        var section = configuration.GetSection("Findx:Authentication:Jwt");
        services.Configure<JwtOptions>(section);
        var jwt = new JwtOptions();
        section.Bind(jwt);
        
        if (!jwt.Enabled) return;

        Check.NotNull(jwt.Secret, nameof(jwt.Secret));

        builder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
        {
            opts.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer ?? "findx",
                ValidateAudience = true,
                ValidAudience = jwt.Audience ?? "findx",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                ValidateLifetime = true,
                LifetimeValidator = (notBefore, expires, securityToken, validationParameters) =>
                {
                    var now = DateTime.UtcNow;
                    var clockSkew = validationParameters.ClockSkew;  // 时钟偏移
    
                    // 检查令牌是否已过期
                    if (expires.HasValue)
                    {
                        var effectiveExpires = expires.Value.Add(clockSkew);
                        if (effectiveExpires < now) return false;
                    }
                    
                    // 检查令牌是否已生效（如果有 notBefore 时间）
                    if (notBefore.HasValue)
                    {
                        var effectiveNotBefore = notBefore.Value.Add(-clockSkew);
                        if (effectiveNotBefore > now) return false;
                    }
                    
                    // 三方令牌检查,如服务端强行管理:下线、锁定、单点等
                    var validator = ServiceLocator.GetService<IAdvancedTokenValidator>();
                    if (validator != null)
                    {
                        return validator.ValidateLifetime(notBefore, expires, securityToken, validationParameters);
                    }
                    
                    return true;
                },
                ClockSkew = TimeSpan.FromMinutes(1), // 1分钟时钟偏移
            };
            
            opts.Events = new FindxJwtBearerEvents();
        });
    }

    /// <summary>
    ///     添加Cookie认证
    /// </summary>
    /// <param name="services"></param>
    /// <param name="builder"></param>
    /// <returns></returns>
    private static void AddCookie(IServiceCollection services, AuthenticationBuilder builder)
    {
        var configuration = services.GetConfiguration();
        
        var cookie = new CookieOptions();
        configuration.GetSection("Findx:Authentication:Cookie").Bind(cookie);
        if (!cookie.Enabled) return;

        builder.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opts =>
        {
            if (cookie.CookieName != null) opts.Cookie.Name = cookie.CookieName;

            opts.LoginPath = cookie.LoginPath ?? opts.LoginPath;
            opts.LogoutPath = cookie.LogoutPath ?? opts.LogoutPath;
            opts.AccessDeniedPath = cookie.AccessDeniedPath ?? opts.AccessDeniedPath;
            opts.ReturnUrlParameter = cookie.ReturnUrlParameter ?? opts.ReturnUrlParameter;
            opts.SlidingExpiration = cookie.SlidingExpiration;

            if (cookie.ExpireMinutes > 0) opts.ExpireTimeSpan = TimeSpan.FromMinutes(cookie.ExpireMinutes);

            opts.EventsType = typeof(FindxCookieAuthenticationEvents);
        });
    }
}