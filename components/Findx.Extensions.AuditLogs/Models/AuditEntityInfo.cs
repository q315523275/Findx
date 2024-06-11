using Findx.Data;

namespace Findx.Extensions.AuditLogs.Models;

/// <summary>
///     审计日志实体变更信息
/// </summary>
public class AuditEntityInfo: EntityBase<Guid>
{
    /// <summary>
    ///     审计Id
    /// </summary>
    public Guid? AuditLogId { set; get; }
    
    /// <summary>
    ///     租户Id
    /// </summary>
    public string TenantId { set; get; }
    
    
    /// <summary>
    ///     变更时间
    /// </summary>
    public DateTime ChangeTime { set; get; }
    
    /// <summary>
    ///     变更类型
    /// </summary>
    public int ChangeType { set; get; }

    
    /// <summary>
    ///     实体记录Id
    /// </summary>
    public string EntityId { set; get; }
    
    /// <summary>
    ///     实体命名空间
    /// </summary>
    public string EntityTypeFullName { set; get; }
    
    /// <summary>
    ///     执行命令
    /// </summary>
    public string CommandText { set; get; }
    
    /// <summary>
    /// 获取或设置 审计实体属性集合
    /// </summary>
    public virtual ICollection<AuditEntityPropertyInfo> Properties { get; set; } = new List<AuditEntityPropertyInfo>();
}
