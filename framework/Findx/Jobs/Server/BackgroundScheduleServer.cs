using System.Threading.Tasks;
using Findx.Jobs.Common;
using Findx.Jobs.Storage;

namespace Findx.Jobs.Server;

/// <summary>
///     调度工作器
/// </summary>
public class BackgroundScheduleServer : BackgroundService, IBackgroundScheduleServer
{
    private readonly ILogger<BackgroundScheduleServer> _logger;
    private readonly IOptions<JobOptions> _options;
    private readonly IJobStorage _storage;
    private readonly IBackgroundTimeWheelServer _backgroundTimeWheelServer;
    private readonly IBackgroundJobTriggerServer _backgroundJobTriggerServer;
    private readonly IBackgroundScheduleElector _backgroundScheduleElector;
    private readonly IJobConverter _jobConverter;
    private readonly ParallelOptions _parallelOptions;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="storage"></param>
    /// <param name="logger"></param>
    /// <param name="backgroundTimeWheelServer"></param>
    /// <param name="backgroundJobTriggerServer"></param>
    /// <param name="jobConverter"></param>
    /// <param name="backgroundScheduleElector"></param>
    public BackgroundScheduleServer(IOptions<JobOptions> options, IJobStorage storage, ILogger<BackgroundScheduleServer> logger, IBackgroundTimeWheelServer backgroundTimeWheelServer, IBackgroundJobTriggerServer backgroundJobTriggerServer, IJobConverter jobConverter, IBackgroundScheduleElector backgroundScheduleElector)
    {
        _options = options;
        _storage = storage;
        _logger = logger;
        _backgroundTimeWheelServer = backgroundTimeWheelServer;
        _backgroundJobTriggerServer = backgroundJobTriggerServer;
        _jobConverter = jobConverter;
        _backgroundScheduleElector = backgroundScheduleElector;
        _parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
    }

    /// <summary>
    ///     循环
    /// </summary>
    /// <param name="stoppingToken"></param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _parallelOptions.CancellationToken = stoppingToken;
        await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            var startTime = DateTime.Now;
            try
            {
                // Master 选举确认
                var isMaster = await _backgroundScheduleElector.PutScheduleLeaderAsync(stoppingToken);
                if (!isMaster) return;
                
                await ExecuteOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调度任务对应内存调度器执行失败");
            }
            // 计算精准的休眠时间,而不是设定固定值，因为执行业务需要耗时
            var costTimeSpan = DateTime.Now - startTime;
            var delayMilliseconds = _options.Value.ScheduleDelay * 1000 - costTimeSpan.TotalMilliseconds;
            if (delayMilliseconds > 0)
            {
                await Task.Delay(Convert.ToInt32(delayMilliseconds), stoppingToken); 
            }
        }
    }

    /// <summary>
    ///     执行
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task ExecuteOnceAsync(CancellationToken cancellationToken)
    {
        var shouldRunJobs = await _storage.FetchShouldRunJobsAsync(DateTimeOffset.Now.AddSeconds(_options.Value.ScheduleDelay), _options.Value.MaxFetchJobCount, cancellationToken);
        var nowTime = DateTimeOffset.Now;
        await Parallel.ForEachAsync(shouldRunJobs, _parallelOptions, async (jobInfo, token) =>
        {
            await DispatchAsync(jobInfo, nowTime, token);
        });
        await _storage.UpdatesAsync(shouldRunJobs, cancellationToken);
    }

    /// <summary>
    ///     任务调度
    /// </summary>
    /// <param name="jobInfo"></param>
    /// <param name="nowTime"></param>
    /// <param name="cancellationToken"></param>
    private async Task DispatchAsync(JobInfo jobInfo, DateTimeOffset nowTime, CancellationToken cancellationToken = default)
    {
        if (nowTime > jobInfo.NextRunTime?.AddSeconds(_options.Value.ScheduleDelay))
        {
            if (_options.Value.MisfireStrategy == JobMisfireStrategy.FireOnceNow)
            {
                await _backgroundJobTriggerServer.TriggerAsync(_jobConverter.ToExecuteInfo(jobInfo), cancellationToken);
            }
            jobInfo.Increment(nowTime);
        }
        else if (nowTime > jobInfo.NextRunTime)
        {
            await _backgroundJobTriggerServer.TriggerAsync(_jobConverter.ToExecuteInfo(jobInfo), cancellationToken);
            jobInfo.Increment(nowTime);
        }
        else 
        {
            // ReSharper disable once PossibleInvalidOperationException
            var ringSecond = jobInfo.NextRunTime.Value.Second;
            _backgroundTimeWheelServer.PushTimeRing(ringSecond , _jobConverter.ToExecuteInfo(jobInfo));
            jobInfo.Increment(jobInfo.NextRunTime.Value);
        }
        while (jobInfo.NextRunTime < nowTime.AddSeconds(_options.Value.ScheduleDelay))
        {
            var ringSecond = jobInfo.NextRunTime.Value.Second;
            _backgroundTimeWheelServer.PushTimeRing(ringSecond , _jobConverter.ToExecuteInfo(jobInfo)) ;
            jobInfo.Increment(jobInfo.NextRunTime.Value);
        }
    }
}