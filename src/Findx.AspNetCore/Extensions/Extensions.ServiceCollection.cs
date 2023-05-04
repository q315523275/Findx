using Findx.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.AspNetCore.Extensions;

public static partial class Extensions
{
    /// <summary>
    ///     添加Razor和读取Razor绑定后内容
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddRazorPageAndRenderer(this IServiceCollection services)
    {
        services.AddRazorPages();
        services.AddTransient<IRazorViewRender, RazorViewRender>();

        return services;
    }

    /// <summary>
    ///     添加跨域访问
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCorsAccessor(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("findx.cors", policyBuilder =>
                policyBuilder.AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetIsOriginAllowed(_ => true)
            );
        });

        return services;
    }
}