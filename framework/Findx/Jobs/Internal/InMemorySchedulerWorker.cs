using System.Threading.Tasks;
using Findx.Extensions;

namespace Findx.Jobs.Internal;

/// <summary>
///     调度工作器
/// </summary>
public class InMemorySchedulerWorker : BackgroundService, IJobSchedulerWorker
{
    private readonly ILogger<InMemorySchedulerWorker> _logger;
    private readonly IOptions<JobOptions> _options;
    private readonly IJobStorage _storage;
    private readonly ITriggerListener _trigger;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="storage"></param>
    /// <param name="trigger"></param>
    /// <param name="logger"></param>
    public InMemorySchedulerWorker(IOptions<JobOptions> options, IJobStorage storage, ITriggerListener trigger, ILogger<InMemorySchedulerWorker> logger)
    {
        _options = options;
        _storage = storage;
        _trigger = trigger;
        _logger = logger;
    }

    /// <summary>
    ///     后台循环执行
    /// </summary>
    /// <param name="stoppingToken"></param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(_options.Value.Delay), stoppingToken);

            try
            {
                await ExecuteOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调度任务对应内存调度器执行失败");
            }
        }
    }

    /// <summary>
    ///     调度执行
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task ExecuteOnceAsync(CancellationToken cancellationToken)
    {
        var shouldRunJobs = await _storage.GetShouldRunJobsAsync(_options.Value.MaxFetchJobCount, cancellationToken);

        foreach (var jobDetail in shouldRunJobs)
        {
            // 当前为内存存储版本，不宜记录过多
            // 直接传递使用作业对象
            // 暂时不使用作业分派记录

            // 内存作业派遣方式
            // 通过内存消息事件或者内存队列推送至执行侧

            // 分布式作业派遣方式
            // 作业派送器主要决定哪些作业需要执行、创建派遣记录
            // 作业触发监听器通过协议(rpc、http、websocket)等来进行作业执行节点通知
            // 作业触发监听器包含作业节点通知、负载、重试、故障转移、并行通知等服务

            // 作业监听器包含节点执行的方式方法，单节点串行执行控制
            // 作业执行者包含作业执行的参数构建等等

            // 固定时间执行任务直接计算下次执行时间
            if (!jobDetail.CronExpress.IsNullOrWhiteSpace() || jobDetail.IsSingle)
                jobDetail.Increment();

            // 固定间隔任务，从推送开始标识执行中
            if (jobDetail.FixedDelay > 0)
                jobDetail.IsRunning = true;

            // 当前使用最简单方式
            await _trigger.TriggerFiredAsync(jobDetail, cancellationToken);
        }
    }
}