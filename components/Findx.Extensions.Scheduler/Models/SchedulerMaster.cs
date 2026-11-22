using Findx.Data;

namespace Findx.Extensions.Scheduler.Models;

/// <summary>
///     调度端租约表（单行，Id 固定为 1）。
///     <para>只用于选出"谁做扫描"以减少无效工作，不作为正确性依据。</para>
/// </summary>
[Table("scheduler_master")]
[EntityExtension(DataSource = "jobs")]
public class SchedulerMaster : FullAuditedBase<long, long>
{
    /// <summary>
    ///     固定主键（单行表）
    /// </summary>
    [Key]
    public override long Id { get; set; }

    /// <summary>
    ///     租约键（多集群共用一个数据源时用于区分，默认 findx:scheduler:master）
    /// </summary>
    public string LockKey { get; set; }

    /// <summary>
    ///     当前持有者标识
    /// </summary>
    public string Owner { get; set; }

    /// <summary>
    ///     租约到期时刻（读侧按它判活）
    /// </summary>
    public DateTime LeaseUntil { get; set; }

    /// <summary>
    ///     最后更新时刻（排障用）
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
