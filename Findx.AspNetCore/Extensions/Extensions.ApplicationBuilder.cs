using Findx.Builders;
using Findx.Extensions;
using Findx.Logging;
using Findx.Modularity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Findx.AspNetCore.Extensions
{
    public static partial class Extensions
    {
        /// <summary>
        /// 注册请求日志中间件
        /// </summary>
        /// <param name="builder">应用程序生成器</param>
        public static IApplicationBuilder UseFindx(this IApplicationBuilder builder)
        {
            #region Findx图标
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
            #endregion

            IServiceProvider provider = builder.ApplicationServices;
            ILogger logger = provider.GetLogger("ApplicationBuilderExtensions");
            logger.LogInformation(0, "框架初始化开始");

            // 打印框架启动日志
            StartupLogger startupLogger = provider.GetService<StartupLogger>();
            startupLogger.Print(provider);

            Stopwatch watch = Stopwatch.StartNew();
            // 框架构建接口
            IFindxBuilder findxBuilder = provider.GetService<IFindxBuilder>();
            IEnumerable<FindxModule> modules = findxBuilder.Modules;
            logger.LogInformation($"共有 {modules.Count()} 个模块需要初始化");
            // 所有模块初始化
            foreach (FindxModule module in findxBuilder.Modules)
            {
                Type moduleType = module.GetType();
                logger.LogInformation($"正在初始化模块《{moduleType.GetDescription()}》({moduleType.Name})”");
                module.OnApplicationInitialization(provider);
                logger.LogInformation($"模块《{moduleType.GetDescription()}》({moduleType.Name})” 初始化完成");
            }
            // 所有模块停止委托注册
            IHostApplicationLifetime hostApplicationLifetime = provider.GetService<IHostApplicationLifetime>();
            hostApplicationLifetime?.ApplicationStopping.Register(() =>
            {
                foreach (var module in findxBuilder.Modules)
                {
                    module.OnApplicationShutdown(provider);
                }
            });

            watch.Stop();
            logger.LogInformation(0, $"框架初始化完成，耗时：{watch.Elapsed}\r\n");

            return builder;
        }
    }
}
