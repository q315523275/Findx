using System;
using System.Diagnostics;
using System.Linq;
using Findx.Builders;
using Findx.Logging;
using Findx.Modularity;
using Findx.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Findx.AspNetCore.Mvc.Middlewares;

namespace Findx.AspNetCore.Extensions
{
	public partial class Extensions
	{
        /// <summary>
        /// 使用Findx框架启动
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static WebApplication UseFindxStartup(this WebApplication app)
        {
            var appInfo = app.Services.GetRequiredService<IApplicationContext>();
            foreach (var uri in appInfo.Uris.Split(";"))
                app.Urls.Add(uri);

            return app;
        }

        /// <summary>
        /// 注册请求日志中间件
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

            var provider = app.Services;
            IApplicationBuilder applicationBuilder = app;
            var logger = provider.GetLogger("ApplicationBuilderExtensions");

            logger.LogInformation(0, "框架初始化开始");

            // 打印框架启动日志
            var startupLogger = provider.GetService<StartupLogger>();
            startupLogger?.Print(provider);

            var watch = Stopwatch.StartNew();
            // 框架构建接口
            var findxBuilder = provider.GetRequiredService<IFindxBuilder>();
            var modules = findxBuilder.Modules;
            logger.LogInformation("共有 {Count} 个模块需要初始化", modules.Count());
            // 所有模块初始化
            foreach (FindxModule module in findxBuilder.Modules)
            {
                var jsTime = DateTime.Now;
                Type moduleType = module.GetType();
                logger.LogInformation("正在初始化模块《{Description}》({ModuleTypeName})”", moduleType.GetDescription(), moduleType.Name);
                if (module is AspNetCoreModuleBase aspNetCoreModule)
                    aspNetCoreModule.UseModule(applicationBuilder);
                else
                    module.UseModule(provider);
                logger.LogInformation("模块《{Description}》({ModuleTypeName})” 初始化完成，耗时{TotalMilliseconds}ms", moduleType.GetDescription(), moduleType.Name, (DateTime.Now - jsTime).TotalMilliseconds);
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

            return app;
        }

        /// <summary>
        /// 添加Json异常处理器中间件
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static WebApplication UseJsonExceptionHandler(this WebApplication app)
        {
            app.UseMiddleware<JsonExceptionHandlerMiddleware>();
            return app;
        }
    }
}
