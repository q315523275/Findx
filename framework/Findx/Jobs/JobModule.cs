using System.ComponentModel;
using Findx.Extensions;
using Findx.Jobs.Internal;
using Findx.Messaging;
using Findx.Modularity;

namespace Findx.Jobs;

/// <summary>
///     Findx-Job内存版本模块
/// </summary>
[Description("Findx-Job内存版本模块")]
public class JobModule : StartupModule
{
    /// <summary>
    ///     Job配置
    /// </summary>
    private JobOptions _options;

    /// <summary>
    ///     等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Framework;

    /// <summary>
    ///     排序
    /// </summary>
    public override int Order => 30;

    /// <summary>
    ///     配置服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        var configuration = services.GetConfiguration();
        var section = configuration.GetSection("Findx:Jobs");
        _options = new JobOptions();
        section.Bind(_options);
        if (_options is not { Enabled: true })
            return services;

        services.Configure<JobOptions>(section);

        // 任务调度
        services.AddSingleton<IJobListener, InMemoryJobListener>();
        services.AddSingleton<IJobScheduler, SimpleJobScheduler>();
        services.AddSingleton<IJobStorage, InMemoryJobStorage>();
        services.AddSingleton<ITriggerListener, InMemoryTriggerListener>();
        services.AddScoped<IApplicationEventHandler<JobInfo>, SimpleJobHandler>();

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

        // 自动载入任务
        services.AddHostedService<JobAutoBuildWorker>();

        return services;
    }
}