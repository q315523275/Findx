using Findx.Data;

namespace Findx.Extensions.Scheduler.Models;

/// <summary>
///     执行账本表（一次触发一行）。
///     <para>唯一键 <c>execution_key</c>（jobId:period:attempt）是"同一周期不重复投递"的第二道保险；
///     第一道是任务行的 NextRunTime CAS。</para>
/// </summary>
[Table("scheduler_execution")]
[EntityExtension(DataSource = "jobs")]
public class SchedulerExecution : FullAuditedBase<long, long>
{
    /// <summary>
    ///     记录编号
    /// </summary>
    [Key]
    public override long Id { get; set; }

    /// <summary>
    ///     执行链路编号（结果回调按它定位并幂等）
    /// </summary>
    public string TraceId { get; set; }

    /// <summary>
    ///     幂等键：jobId:period:attempt，唯一索引
    /// </summary>
    public string ExecutionKey { get; set; }

    /// <summary>
    ///     任务编号
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
    ///     执行参数（原样透传给 Worker）
    /// </summary>
    public string Parameter { get; set; }

    /// <summary>
    ///     周期标识（周期触发为到期时刻 O 格式；重试为 R: 前缀）
    /// </summary>
    public string Period { get; set; }

    /// <summary>
    ///     尝试序号（0=首次）
    /// </summary>
    public int Attempt { get; set; }

    /// <summary>
    ///     执行状态
    /// </summary>
    public ExecutionState State { get; set; }

    /// <summary>
    ///     执行节点地址（未受理时为空）
    /// </summary>
    public string WorkerAddress { get; set; }

    /// <summary>
    ///     分发/领取时刻（超时兜底据此判定）
    /// </summary>
    public DateTime? DispatchAt { get; set; }

    /// <summary>
    ///     终结时刻
    /// </summary>
    public DateTime? FinishAt { get; set; }

    /// <summary>
    ///     执行耗时（毫秒，回调回传）
    /// </summary>
    public long? ElapsedMs { get; set; }

    /// <summary>
    ///     失败原因
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    ///     执行结果消息（JobResult.Message，无论成功失败都记录）
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     执行过程日志（执行端按 TraceId 捕获的日志片段，超长时保留尾部；由调度端落库便于事后排查）
    ///     <para>显式指定 mediumtext：不声明类型时 FreeSql 会按 varchar(255) 建列，
    ///     几百字的日志尾部直接把回调写挂（Data too long for column 'JobLog'）。</para>
    /// </summary>
    [Column(TypeName = "mediumtext")]
    public string JobLog { get; set; }
}
