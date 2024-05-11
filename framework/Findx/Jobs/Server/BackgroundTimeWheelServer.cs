using System.Threading.Tasks;

namespace Findx.Jobs.Server;

/// <summary>
///     时间轮处理服务
/// </summary>
public class BackgroundTimeWheelServer: BackgroundService, IBackgroundTimeWheelServer
{
    private static readonly ConcurrentDictionary<long, List<JobExecuteInfo>> TimeWheel = new();
    private readonly ILogger<BackgroundTimeWheelServer> _logger;
    private readonly IBackgroundJobTriggerServer _backgroundJobTriggerServer;

    /// <summary>
    ///     
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="backgroundJobTriggerServer"></param>
    public BackgroundTimeWheelServer(ILogger<BackgroundTimeWheelServer> logger, IBackgroundJobTriggerServer backgroundJobTriggerServer)
    {
        _logger = logger;
        _backgroundJobTriggerServer = backgroundJobTriggerServer;
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="ringSecond"></param>
    /// <param name="jobExecuteInfo"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void PushTimeRing(int ringSecond, JobExecuteInfo jobExecuteInfo)
    {
        if (!TimeWheel.ContainsKey(ringSecond))
        {
            TimeWheel[ringSecond] = [];
        }
        TimeWheel[ringSecond].Add(jobExecuteInfo);
    }

    /// <summary>
    ///     执行器
    /// </summary>
    /// <param name="stoppingToken"></param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //Console.WriteLine(TimeWheel.ToJson());
                var nowTime = DateTime.Now;
                var nowSecond = nowTime.Second;
                var ringItemData = new List<JobExecuteInfo>();
                // 获取当前所处的一分钟第几秒，然后 for 两次，第二次是为了重跑前面一个刻度没有被执行的的 job list ，避免前面的刻度遗漏了
                for (var i = 0 ; i < 2 ; i++) 
                {
                    if (TimeWheel.TryRemove((nowSecond+ 60 -i) % 60, out var jobs))
                    {
                        ringItemData.AddRange(jobs);
                    }
                }
                // 推送任务触发器服务
                foreach (var item in ringItemData)
                {
                    await _backgroundJobTriggerServer.TriggerAsync(item, stoppingToken);
                }
                // 休眠
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "时间轮执行失败");
        }
    }
}