using Findx.Data;

namespace Findx.Extensions.Scheduler.Models;

/// <summary>
///     调度任务死信表（超过最大重试次数后写入，等待人工介入）
/// </summary>
[Table("scheduler_job_dead_letter")]
[EntityExtension(DataSource = "jobs")]
public class JobDeadLetter : FullAuditedBase<long, long>
{
    /// <summary>
    ///     记录编号
    /// </summary>
    [Key]
    [Column(name: "id")]
    public override long Id { get; set; }

    /// <summary>
    ///     调度任务编号
    /// </summary>
    public long JobId { get; set; }

    /// <summary>
    ///     任务逻辑名
    /// </summary>
    public string JobName { get; set; }

    /// <summary>
    ///     作业实现类全名
    /// </summary>
    public string HandlerType { get; set; }

    /// <summary>
    ///     执行参数（JSON）
    /// </summary>
    public string Parameter { get; set; }

    /// <summary>
    ///     Cron 表达式
    /// </summary>
    public string CronExpression { get; set; }

    /// <summary>
    ///     失败时已尝试次数
    /// </summary>
    public int TryCount { get; set; }

    /// <summary>
    ///     最后错误信息
    /// </summary>
    public string LastError { get; set; }

    /// <summary>
    ///     最后执行节点地址
    /// </summary>
    public string WorkerAddress { get; set; }

    /// <summary>
    ///     触发时间（服务器本地时间）
    /// </summary>
    public DateTime? FiredTime { get; set; }

    /// <summary>
    ///     执行链路编号
    /// </summary>
    public string TraceId { get; set; }
}
