﻿using System;
using System.Diagnostics;
using System.Linq;
using Findx.AspNetCore.Mvc.Middlewares;
using Findx.Builders;
using Findx.Extensions;
using Findx.Logging;
using Findx.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Findx.AspNetCore.Extensions;

public partial class Extensions
{
    /// <summary>
    ///     使用Findx框架启动
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication UseFindxStartup(this WebApplication app)
    {
        var appInfo = app.Services.GetRequiredService<IApplicationContext>();
        foreach (var uri in appInfo.Uris.Split(";")) app.Urls.Add(uri);
        return app;
    }
    
    /// <summary>
    ///     使用Findx框架启动
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static void UseFindxHosting(this WebApplication app)
    {
        var appInfo = app.Services.GetRequiredService<IApplicationContext>();
        foreach (var uri in appInfo.Uris.Split(";")) app.Urls.Add(uri);
        app.Run();
    }

    /// <summary>
    ///     注册请求日志中间件
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication UseFindx(this WebApplication app)
    {
        #region Findx图标

        var defaultColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.WriteLine(@"
    #########################################################
    ##       ___________.__            .___                ##
    ##       \_   _____/|__| ____    __| _/__  ___         ##
    ##        |    __)  |  |/    \  / __ |\  \/  /         ##
    ##        |     \   |  |   |  \/ /_/ | >    <          ##
    ##        \___  /   |__|___|  /\____ |/__/\_ \         ##
    ##            \/            \/      \/      \/         ##
    #########################################################");
        Console.WriteLine();
        Console.ForegroundColor = defaultColor;

        #endregion

        #region Findx模块构建
        var provider = app.Services;
        var logger = provider.GetLogger("ApplicationBuilderExtensions");

        logger.LogInformation(0, "框架初始化开始");

        // 打印框架启动日志
        var startupLogger = provider.GetService<StartupLogger>();
        startupLogger?.Print(provider);

        var watch = Stopwatch.StartNew();
        
        // 框架构建接口
        var findxBuilder = provider.GetRequiredService<IFindxBuilder>();
        var modules = findxBuilder.Modules;
        logger.LogInformation("共有 {Count} 个模块需要初始化{Line}", modules.Count(), CommonUtility.Line);
        
        // 所有模块初始化
        foreach (var module in findxBuilder.Modules)
        {
            var jsTime = DateTime.Now;
            var moduleType = module.GetType();
            logger.LogInformation("正在初始化模块《{Description}》({ModuleTypeName})”", moduleType.GetDescription(), moduleType.Name);
            if (module is WebApplicationModuleBase webApplicationModule)
                webApplicationModule.UseModule(app);
            // else if (module is AspNetCoreModuleBase aspNetCoreModule)
            //     aspNetCoreModule.UseModule(app);
            else
                module.UseModule(provider);
            logger.LogInformation("模块《{Description}》({ModuleTypeName})” 初始化完成，耗时{TotalMilliseconds}ms",
                moduleType.GetDescription(), moduleType.Name, (DateTime.Now - jsTime).TotalMilliseconds);
        }

        // 所有模块停止委托注册
        var applicationStopping = app.Lifetime.ApplicationStopping;
        applicationStopping.Register(() =>
        {
            foreach (var module in findxBuilder.Modules) module.OnShutdown(provider);
        });

        watch.Stop();
        logger.LogInformation(0, "框架初始化完成，耗时:{ElapsedTotalMilliseconds}毫秒，进程编号:{ProcessId}{Line}", watch.Elapsed.TotalMilliseconds, Environment.ProcessId, CommonUtility.Line);
        #endregion
        
        return app;
    }
    
    /// <summary>
    ///     添加Json异常处理器中间件
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication UseEndpointsWithAreaRoute(this WebApplication app)
    {
        app.UseEndpoints(builder =>
        {
            builder.MapControllerRoute("areas-router", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            builder.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
        });
        return app;
    }

    /// <summary>
    ///     添加Json异常处理器中间件
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication UseJsonExceptionHandler(this WebApplication app)
    {
        app.UseMiddleware<JsonExceptionHandlerMiddleware>();
        return app;
    }

    /// <summary>
    ///     添加跟踪标识中间件
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication UseCorrelationId(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        return app;
    }
    
    /// <summary>
    ///     使用跨域请求
    /// </summary>
    /// <param name="app"></param>
    /// <param name="policyName">策略名称</param>
    /// <returns></returns>
    public static WebApplication UseCorsAccessor(this WebApplication app, string policyName = "findx.cors")
    {
        app.UseCors(policyName);
        return app;
    }
}