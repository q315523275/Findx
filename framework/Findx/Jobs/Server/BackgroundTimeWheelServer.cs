using System.Threading.Tasks;

namespace Findx.Jobs.Server;

/// <summary>
///     时间轮处理服务
/// </summary>
public class BackgroundTimeWheelServer: BackgroundService, IBackgroundTimeWheelServer
{
    //  实例字段：JobModule 已保证注入方与 hosted 消费方为同一实例
    private readonly ConcurrentDictionary<long, List<JobExecuteInfo>> _timeWheel = new();
    //  上一轮 tick 处理到的秒槽位（-1 表示尚未开始）
    private int _lastSecond = -1;
    //  上一轮 tick 的实际时刻：判定停顿是否跨过整圈（≥60s 时仅凭秒号无法区分 1 秒与 61 秒）
    private DateTime _lastTickTime;
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
    ///      推送时间轮任务
    /// </summary>
    /// <param name="ringSecond"></param>
    /// <param name="jobExecuteInfo"></param>
    public void PushTimeRing(int ringSecond, JobExecuteInfo jobExecuteInfo)
    {
        //  AddOrUpdate 保证槽位创建原子；取出桶引用后对 List 加锁再 Add，
        //  避免 check-then-act 覆盖丢任务与并发 Add 破坏 List 内部状态
        var bucket = _timeWheel.AddOrUpdate(ringSecond, _ => [], (_, list) => list);
        lock (bucket)
        {
            bucket.Add(jobExecuteInfo);
        }
    }

    /// <summary>
    ///     执行器
    /// </summary>
    /// <param name="stoppingToken"></param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var nowTime = DateTime.Now;
                var nowSecond = nowTime.Second;
                var ringItemData = new List<JobExecuteInfo>();

                //  首次启动：只初始化游标，本轮不消费——
                //  若调度器先于本服务入桶了未来秒任务，扫任何槽都会把它们提前触发
                if (_lastSecond < 0)
                {
                    _lastSecond = nowSecond;
                    _lastTickTime = nowTime;
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }

                //  自上次 tick 经过的秒数（含当前秒），最少为 1：
                //   - 停顿 < 60s：扫描集 = 自 lastSecond+1 走到 nowSecond 的前向路径槽（均已于此刻之前到期），
                //     中间槽当轮补消费，不延迟到下一圈
                //   - 停顿 ≥ 60s（跨圈）：全部 60 槽均已过期，扫整圈当轮补全；同槽跨圈的多批任务合并触发
                //    （60 槽单圈时间轮的固有语义，保证"至少触发一次"）
                //   - 绝不主动扫 nowSecond 之后的槽：调度器按未来秒提前入桶，正常 tick 下扫未来槽会提前触发任务
                var elapsed = (int)(nowTime - _lastTickTime).TotalSeconds;
                if (elapsed < 1) elapsed = 1;
                var scanCount = Math.Min(60, elapsed);
                for (var i = 0; i < scanCount; i++)
                {
                    var slot = (_lastSecond + 1 + i) % 60;
                    if (_timeWheel.TryRemove(slot, out var jobs))
                    {
                        ringItemData.AddRange(jobs);
                    }
                }
                _lastSecond = nowSecond;
                _lastTickTime = nowTime;

                //  推送任务触发器服务
                foreach (var item in ringItemData)
                {
                    await _backgroundJobTriggerServer.TriggerAsync(item, stoppingToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "时间轮执行失败");
            }
            // 休眠
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}