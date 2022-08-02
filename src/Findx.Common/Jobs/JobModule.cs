﻿using System.ComponentModel;
using System.Threading.Tasks;
using Findx.Extensions;
using Findx.Jobs.Local;
using Findx.Messaging;
using Findx.Modularity;

namespace Findx.Jobs
{
    /// <summary>
    /// Findx-Job内存版本模块
    /// </summary>
	[Description("Findx-Job内存版本模块")]
	public class JobModule : FindxModule
	{
        /// <summary>
        /// 等级
        /// </summary>
		public override ModuleLevel Level => ModuleLevel.Framework;

        /// <summary>
        /// 排序
        /// </summary>
		public override int Order => 20;

        /// <summary>
        /// Job配置
        /// </summary>
        private JobOptions _options;

        /// <summary>
        /// 配置服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            var configuration = services.GetConfiguration();
            var section = configuration.GetSection("Findx:Jobs");
            _options = section.Get<JobOptions>();
            if (_options is not { Enabled: true })
                return services;

            services.Configure<JobOptions>(section);

            // 任务调度
            services.AddSingleton<IJobListener, InMemoryJobListener>();
            services.AddSingleton<IJobScheduler, DefaultJobScheduler>();
            services.AddSingleton<IJobStorage, InMemoryJobStorage>();
            services.AddSingleton<ITriggerListener, InMemoryTriggerListener>();
            services.AddScoped<IApplicationEventHandler<JobInfo>, LocalJobHandler>();

            // 自动注册作业
            var dict = new JobTypeDictionary();
            var jobFinder = services.GetOrAddTypeFinder<IJobFinder>(assemblyFinder => new JobFinder(assemblyFinder));
            var jobTypes = jobFinder.FindAll(true);
            foreach (var jobType in jobTypes)
            {
                services.AddTransient(jobType);
                if (jobType.FullName != null) 
                    dict.Add(jobType.FullName, jobType);
            }
            services.AddSingleton(dict);

            // 调度工作者
            services.AddHostedService<InMemorySchedulerWorker>();

            return services;
        }
        
        /// <summary>
        /// 线程通知
        /// </summary>
        private CancellationTokenSource CancellationToken { get; set; }

        /// <summary>
        /// 启用模块
        /// </summary>
        /// <param name="provider"></param>
        public override void UseModule(IServiceProvider provider)
        {
            if (_options is not { Enabled: true }) return;
            CancellationToken = new CancellationTokenSource();
            Task.Run(() =>
            {
                var scheduler = provider.GetRequiredService<IJobScheduler>();
                var jobFinder = provider.GetRequiredService<IJobFinder>();
                var jobTypes = jobFinder.FindAll(true);

                foreach (var jobType in jobTypes)
                {
                    // 需要自动载入执行的任务
                    if (jobType.HasAttribute<JobAttribute>())
                    {
                        scheduler.ScheduleAsync(jobType);
                    }
                }

            }, CancellationToken.Token);
            base.UseModule(provider);
        }

        /// <summary>
        /// 应用销毁
        /// </summary>
        /// <param name="provider"></param>
        public override void OnShutdown(IServiceProvider provider)
        {
            if (_options is { Enabled: true })
            {
                CancellationToken.Cancel();
            }
            base.OnShutdown(provider);
        }
    }
}

