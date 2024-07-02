using System.Linq;
using Findx.AspNetCore.Mvc;
using Findx.AspNetCore.Options;
using Findx.Extensions;
using Microsoft.Extensions.Configuration;
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
        // 配置服务
        var corsOptions = new CorsOptions();
        var configuration = services.GetConfiguration();
        var section = configuration.GetSection("Findx:Cors");
        section.Bind(corsOptions);
        
        services.AddCors(options =>
        {
            options.AddPolicy("findx.cors", policyBuilder =>
            {
                if (corsOptions.WithHeaders.Any())
                    policyBuilder.WithHeaders(corsOptions.WithHeaders);
                else 
                    policyBuilder.AllowAnyHeader();
                
                if (corsOptions.WithMethods.Any())
                    policyBuilder.WithMethods(corsOptions.WithMethods);
                else 
                    policyBuilder.AllowAnyMethod();
                
                if (corsOptions.AllowCredentials)
                    policyBuilder.AllowCredentials();
                else if (corsOptions.DisallowCredentials)
                    policyBuilder.DisallowCredentials();
                
                if (corsOptions.WithOrigins.Any()) 
                    policyBuilder.WithOrigins(corsOptions.WithOrigins);
                else 
                    policyBuilder.AllowAnyOrigin();
            });
        });

        return services;
    }
}