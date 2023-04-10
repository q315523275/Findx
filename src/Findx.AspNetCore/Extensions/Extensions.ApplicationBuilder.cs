using Findx.AspNetCore.Mvc.Middlewares;
using Findx.Builders;
using Findx.Extensions;
using Findx.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;

namespace Findx.AspNetCore.Extensions
{
    /// <summary>
    /// AspNetCore应用扩展
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 添加MVC并支持Area路由
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
        /// 添加Endpoint并Area路由支持
        /// </summary>
        public static IEndpointRouteBuilder MapControllersWithAreaRoute(this IEndpointRouteBuilder endpoints, bool area = true)
        {
            if (area)
            {
                endpoints.MapControllerRoute(
                    name: "areas-router",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            }

            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            return endpoints;
        }

        /// <summary>
        /// 注册请求日志中间件
        /// </summary>
        /// <param name="builder">应用程序生成器</param>
        public static IApplicationBuilder UseFindx(this IApplicationBuilder builder)
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

            var provider = builder.ApplicationServices;
            var logger = provider.GetLogger("ApplicationBuilderExtensions");
            logger.LogInformation(0, "框架初始化开始");

            // 打印框架启动日志
            var startupLogger = provider.GetService<StartupLogger>();
            startupLogger?.Print(provider);

            var watch = Stopwatch.StartNew();
            // 框架构建接口
            var findxBuilder = provider.GetRequiredService<IFindxBuilder>();
            var modules = findxBuilder.Modules;
            logger.LogInformation(message: $"共有 {modules.Count()} 个模块需要初始化");
            // 所有模块初始化
            foreach (var module in findxBuilder.Modules)
            {
                var jsTime = DateTime.Now;
                var moduleType = module.GetType();
                logger.LogInformation(message: $"正在初始化模块《{moduleType.GetDescription()}》({moduleType.Name})”");
                if (module is AspNetCoreModuleBase aspNetCoreModule)
                    aspNetCoreModule.UseModule(builder);
                else
                    module.UseModule(provider);
                logger.LogInformation($"模块《{moduleType.GetDescription()}》({moduleType.Name})” 初始化完成，耗时{(DateTime.Now - jsTime).TotalMilliseconds}ms");
            }
            // 所有模块停止委托注册
            var hostApplicationLifetime = provider.GetService<IHostApplicationLifetime>();
            hostApplicationLifetime?.ApplicationStopping.Register(() =>
            {
                foreach (var module in findxBuilder.Modules)
                {
                    module.OnShutdown(provider);
                }
            });

            watch.Stop();
            logger.LogInformation(0, $"框架初始化完成，耗时:{watch.Elapsed.TotalMilliseconds}毫秒，进程编号:{Environment.ProcessId}\r\n");

            return builder;
        }

        /// <summary>
        /// 添加Json异常处理器中间件
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseJsonExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JsonExceptionHandlerMiddleware>();
        }
    }
}
