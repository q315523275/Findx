using System;
using System.Diagnostics;
using System.Linq;
using Findx.AspNetCore.Mvc.Middlewares;
using Findx.Builders;
using Findx.Extensions;
using Findx.Logging;
using Findx.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Findx.AspNetCore.Extensions;

/// <summary>
///     AspNetCore应用扩展
/// </summary>
public static partial class Extensions
{
    /// <summary>
    ///     添加MVC并支持Area路由
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <param name="area">是否支持Area路由</param>
    public static IApplicationBuilder UseMvcWithAreaRoute(this IApplicationBuilder app, bool area = true)
    {
        return app.UseMvc(builder =>
        {
            if (area)
                builder.MapRoute("area", "{area:exists}/{controller}/{action=Index}/{id?}");
            builder.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
        });
    }

    /// <summary>
    ///     添加Endpoint并Area路由支持
    /// </summary>
    public static IEndpointRouteBuilder MapControllersWithAreaRoute(this IEndpointRouteBuilder endpoints, bool area = true)
    {
        if (area)
            endpoints.MapControllerRoute("areas-router", "{area:exists}/{controller=Home}/{action=Index}/{id?}");

        endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

        return endpoints;
    }

    /// <summary>
    ///     注册请求日志中间件
    /// </summary>
    /// <param name="builder">应用程序生成器</param>
    [Obsolete("Invalid. Please use code UseFindx(this WebApplication app))")]
    public static IApplicationBuilder UseFindx(this IApplicationBuilder builder)
    {
        return builder;
    }

    /// <summary>
    ///     添加Json异常处理器中间件
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseJsonExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JsonExceptionHandlerMiddleware>();
    }
    
    /// <summary>
    ///     添加跟踪标识中间件
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }

    /// <summary>
    ///     使用跨域请求
    /// </summary>
    /// <param name="app"></param>
    /// <param name="policyName">策略名称</param>
    /// <returns></returns>
    public static IApplicationBuilder UseCorsAccessor(this IApplicationBuilder app, string policyName = "findx.cors")
    {
        app.UseCors(policyName);
        return app;
    }
}