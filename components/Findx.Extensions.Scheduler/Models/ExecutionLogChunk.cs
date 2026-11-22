using Findx.Data;

namespace Findx.Extensions.Scheduler.Models;

/// <summary>
///     执行日志分片（执行端 -> 调度端，POST /api/scheduler/jobs/logs）。
///     <para>日志量大时不随结果回调一次性传：执行过程中按条数阈值切片，经发送队列批量上报；
///     调度端按 (TraceId, Sequence) 追加落库，管理面可分页查看完整过程。</para>
/// </summary>
public class ExecutionLogChunk
{
    /// <summary>
    ///     执行链路编号
    /// </summary>
    public string TraceId { get; set; }

    /// <summary>
    ///     任务编号
    /// </summary>
    public long JobId { get; set; }

    /// <summary>
    ///     分片序号（同一次执行内从 0 递增，落库后按它排序）
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    ///     分片内容（按行拼接，单片字符数有上限）
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    ///     是否本次执行最后一片（管理面据此判断日志是否完整）
    /// </summary>
    public bool IsFinal { get; set; }

    /// <summary>
    ///     采集时刻
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    ///     调度端地址（执行端上报时带上，避免依赖静态配置）
    /// </summary>
    public string MasterAddress { get; set; }
}

/// <summary>
///     执行日志明细表（一次执行多行，按 Id / Sequence 递增）
/// </summary>
[Table("scheduler_execution_log")]
[EntityExtension(DataSource = "jobs")]
public class SchedulerExecutionLog : FullAuditedBase<long, long>
{
    /// <summary>
    ///     记录编号（自增，管理面按它做游标分页）
    /// </summary>
    [Key]
    public override long Id { get; set; }

    /// <summary>
    ///     执行链路编号
    /// </summary>
    public string TraceId { get; set; }

    /// <summary>
    ///     任务编号
    /// </summary>
    public long JobId { get; set; }

    /// <summary>
    ///     分片序号
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    ///     分片内容（单片 ≤8000 字符，显式声明 text，避免被建成 varchar(255)）
    /// </summary>
    [Column(TypeName = "text")]
    public string Content { get; set; }

    /// <summary>
    ///     是否最后一片
    /// </summary>
    public bool IsFinal { get; set; }
}
