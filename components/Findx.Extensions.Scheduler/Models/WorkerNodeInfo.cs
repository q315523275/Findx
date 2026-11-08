namespace Findx.Extensions.Scheduler.Models;

/// <summary>
///     Worker 节点心跳信息（写入 Redis，Master 侧按 Capabilities 反查可执行节点）
/// </summary>
public class WorkerNodeInfo
{
    /// <summary>
    ///     节点唯一编号（由 NodeName 推导，重启保持稳定）
    /// </summary>
    public string WorkerId { get; set; }

    /// <summary>
    ///     节点显示名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     节点对外地址（http://host:port），Master 向其 POST 分发
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    ///     分发权重（扩展位：当前 RoundRobin 策略未使用，预留加权分发）
    /// </summary>
    public int Weight { get; set; } = 1;

    /// <summary>
    ///     该节点可执行的任务清单（JobName 或 IJob 实现类 FullName）
    /// </summary>
    public List<string> Capabilities { get; set; } = [];

    /// <summary>
    ///     最近心跳时间
    /// </summary>
    public DateTime HeartbeatTime { get; set; }
}
