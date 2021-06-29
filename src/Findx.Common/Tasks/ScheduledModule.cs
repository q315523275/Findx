using Findx.Extensions;
using Findx.Modularity;
using Findx.Tasks.Scheduling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Threading;

namespace Findx.Tasks
{
    [Description("Findx-内嵌调度模块")]
    public class ScheduledModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Framework;

        public override int Order => 20;

        private SchedulerOptions SchedulerOptions { set; get; }

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            // 配置服务
            IConfiguration configuration = services.GetConfiguration();
            var section = configuration.GetSection("Findx:Scheduler");
            SchedulerOptions = section.Get<SchedulerOptions>();
            services.Configure<SchedulerOptions>(section);
            if (SchedulerOptions == null || !SchedulerOptions.Enable)
                return services;

            // 任务调度
            services.AddSingleton<IScheduledTaskManager, InMemoryScheduledTaskManager>();
            services.AddSingleton<IScheduledTaskStore, InMemoryScheduledTaskStore>();
            services.AddSingleton<IScheduledTaskExecuter, InMemoryScheduledTaskExecuter>();
            services.AddSingleton<IScheduler, InMemoryScheduler>();
            services.AddSingleton<SchedulerTaskWrapperDictionary>();

            // 后台任务
            // services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            // services.AddHostedService<AsyncQueueTaskHostedService>();

            // 自动注册任务
            IScheduledTaskFinder scheduledTaskFinder = services.GetOrAddTypeFinder<IScheduledTaskFinder>(assemblyFinder => new ScheduledTaskFinder(assemblyFinder));
            Type[] scheduledTaskTypes = scheduledTaskFinder.FindAll();
            foreach (Type scheduledTaskType in scheduledTaskTypes)
            {
                services.AddTransient(scheduledTaskType);
            }

            return services;
        }

        protected CancellationTokenSource cancellationToken { get; private set; }

        public override void UseModule(IServiceProvider provider)
        {
            if (SchedulerOptions != null && SchedulerOptions.Enable)
            {
                IScheduler scheduler = provider.GetRequiredService<IScheduler>();
                IScheduledTaskManager scheduledTaskManager = provider.GetRequiredService<IScheduledTaskManager>();
                IScheduledTaskFinder scheduledTaskFinder = provider.GetRequiredService<IScheduledTaskFinder>();

                Type[] scheduledTaskTypes = scheduledTaskFinder.FindAll();

                foreach (Type scheduledTaskType in scheduledTaskTypes)
                {
                    // 需要自带执行的任务
                    if (scheduledTaskType.HasAttribute<ScheduledAttribute>())
                    {
                        var schedulerTaskWrapper = new SchedulerTaskWrapper(scheduledTaskType);
                        scheduledTaskManager.ScheduleAsync(schedulerTaskWrapper);
                    }
                }

                cancellationToken = new CancellationTokenSource();
                scheduler.StartAsync(cancellationToken.Token);

                base.UseModule(provider);
            }
        }

        public override void OnShutdown(IServiceProvider provider)
        {
            if (SchedulerOptions != null && SchedulerOptions.Enable)
            {
                cancellationToken.Cancel();

                IScheduler scheduler = provider.GetRequiredService<IScheduler>();

                scheduler?.StopAsync(cancellationToken.Token).ConfigureAwait(false).GetAwaiter();
            }

            base.OnShutdown(provider);
        }
    }
}
