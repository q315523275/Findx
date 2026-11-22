using Findx.Data;

namespace Findx.Extensions.Scheduler.Models;

/// <summary>
///     Worker 节点注册表（执行端 HTTP 心跳上报，调度端读表路由）。
/// </summary>
[Table("scheduler_worker")]
[EntityExtension(DataSource = "jobs")]
public class SchedulerWorker : FullAuditedBase<long, long>
{
    /// <summary>
    ///     记录编号
    /// </summary>
    [Key]
    public override long Id { get; set; }

    /// <summary>
    ///     节点唯一编号（由 NodeName 推导，重启保持稳定），唯一索引
    /// </summary>
    public string WorkerId { get; set; }

    /// <summary>
    ///     节点显示名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     节点对外地址（http://host:port）
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    ///     分发权重（当前轮询策略未使用，预留加权分发）
    /// </summary>
    public int Weight { get; set; } = 1;

    /// <summary>
    ///     能力清单（JSON 数组，元素为 IJob 实现类全名）
    /// </summary>
    public string Capabilities { get; set; }

    /// <summary>
    ///     最近心跳时刻
    /// </summary>
    public DateTime HeartbeatAt { get; set; }
}
