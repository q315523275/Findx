using Findx.Data;
using Findx.Jobs.Common;

namespace Findx.Extensions.Scheduler.Models;

/// <summary>
///     调度任务定义表（持久化落库，Master 统一读取与推进）
/// </summary>
[Table("scheduler_job")]
[EntityExtension(DataSource = "jobs")]
public class ScheduledJob : FullAuditedBase<long, long>
{
    /// <summary>
    ///     记录编号
    /// </summary>
    [Key]
    [Column(name: "id")]
    public override long Id { get; set; }

    /// <summary>
    ///     任务逻辑名（Worker 能力匹配键之一，建议业务唯一）
    /// </summary>
    public string JobName { get; set; }

    /// <summary>
    ///     作业实现类全名（Findx.Jobs.IJob 实现，Worker 能力匹配键之一）
    /// </summary>
    public string HandlerType { get; set; }

    /// <summary>
    ///     Cron 表达式（支持 5 段或 6 段含秒）
    /// </summary>
    public string CronExpression { get; set; }

    /// <summary>
    ///     执行参数（JSON 字符串，原样透传）
    /// </summary>
    public string Parameter { get; set; }

    /// <summary>
    ///     任务状态（1=正常调度，2=停止）
    /// </summary>
    public JobStatus Status { get; set; } = JobStatus.Normal;

    /// <summary>
    ///     最近一次分发时间（服务器本地时间）
    /// </summary>
    public DateTime? LastRunTime { get; set; }

    /// <summary>
    ///     下次应触发时间（服务器本地时间），Master 通过 CAS 抢占推进；过期即补触发一次。
    ///     该字段只承载「正常周期锚点」，失败重试不再改写它，避免重试后 cron 节奏偏移
    /// </summary>
    public DateTime? NextRunTime { get; set; }

    /// <summary>
    ///     失败重试的到期时刻（服务器本地时间），与 NextRunTime 解耦：重试到点由 Master CAS 抢占清空；
    ///     成功后周期锚点（NextRunTime）保持原推进结果，不再被退避时刻覆盖
    /// </summary>
    public DateTime? RetryAtTime { get; set; }

    /// <summary>
    ///     当前周期已连续失败次数（成功后归零）
    /// </summary>
    public int TryCount { get; set; }

    /// <summary>
    ///     备注
    /// </summary>
    public string Remark { get; set; }
}
