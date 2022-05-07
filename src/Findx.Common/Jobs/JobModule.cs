using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Findx.Extensions;
using Findx.Jobs.Local;
using Findx.Messaging;
using Findx.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.Jobs
{
	[Description("Findx-Job内存版本模块")]
	public class JobModule : FindxModule
	{
		public override ModuleLevel Level => ModuleLevel.Framework;

		public override int Order => 100;

        private JobOptions Options;

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            // 配置服务
            IConfiguration configuration = services.GetConfiguration();
            var section = configuration.GetSection("Findx:Jobs");
            Options = section.Get<JobOptions>();
            if (Options == null || !Options.Enabled)
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
            IJobFinder jobFinder = services.GetOrAddTypeFinder<IJobFinder>(assemblyFinder => new JobFinder(assemblyFinder));
            var jobTypes = jobFinder.FindAll(true);
            foreach (Type jobType in jobTypes)
            {
                services.AddTransient(jobType);
                dict.Add(jobType.FullName, jobType);
            }
            services.AddSingleton(dict);

            // 调度工作者
            services.AddHostedService<InMemorySchedulerWorker>();

            return services;
        }

        protected CancellationTokenSource cancellationToken { get; private set; }

        public override void UseModule(IServiceProvider provider)
        {
            if (Options != null && Options.Enabled)
            {
                cancellationToken = new CancellationTokenSource();
                Task.Run(() =>
                {
                    var scheduler = provider.GetRequiredService<IJobScheduler>();
                    IJobFinder jobFinder = provider.GetRequiredService<IJobFinder>();
                    var jobTypes = jobFinder.FindAll(true);

                    foreach (Type jobType in jobTypes)
                    {
                        // 需要自动载入执行的任务
                        if (jobType.HasAttribute<JobAttribute>())
                        {
                            scheduler.ScheduleAsync(jobType);
                        }
                    }

                }, cancellationToken.Token);
                base.UseModule(provider);
            }
        }

        public override void OnShutdown(IServiceProvider provider)
        {
            if (Options != null && Options.Enabled)
            {
                cancellationToken.Cancel();
            }

            base.OnShutdown(provider);
        }

    }
}

