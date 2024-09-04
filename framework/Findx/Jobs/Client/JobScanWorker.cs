using System.Threading.Tasks;
using Findx.Extensions;

namespace Findx.Jobs.Client;

/// <summary>
///     任务自动构建工作器
/// </summary>
public class JobScanWorker : BackgroundService
{
    private readonly IJobFinder _jobFinder;
    private readonly IJobScheduler _scheduler;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="scheduler"></param>
    /// <param name="jobFinder"></param>
    public JobScanWorker(IJobScheduler scheduler, IJobFinder jobFinder)
    {
        _scheduler = scheduler;
        _jobFinder = jobFinder;
    }

    /// <summary>
    ///     执行
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jobTypes = _jobFinder.FindAll(true);

        foreach (var jobType in jobTypes)
            // 需要自动载入执行的任务
            if (jobType.HasAttribute<JobAttribute>())
                _scheduler.ScheduleAsync(jobType, stoppingToken);

        return Task.CompletedTask;
    }
}