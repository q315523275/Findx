using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Findx.Data;

namespace Findx.Extensions.AuditLogs.Models;

/// <summary>
///     审计日志实体变更信息
/// </summary>
[Table("FindxEntityChanges")]
[EntityExtension(DataSource = "AuditLog", DisableAuditing = true)]
[Description("审计操作信息")]
public class AuditEntityInfo: EntityBase<Guid>
{
    /// <summary>
    ///     日志编号
    /// </summary>
    public Guid AuditLogId { get; set; }
    
    /// <summary>
    ///     获取或设置 数据编号
    /// </summary>
    public virtual string EntityId { get; set; }
    
    /// <summary>
    ///     获取或设置 实体名称
    /// </summary>
    public virtual string EntityTypeName { get; set; }
    
    /// <summary>
    ///     获取或设置 实体全称
    /// </summary>
    public virtual string EntityTypeFullName { get; set; }

    /// <summary>
    ///     获取或设置 执行时间
    /// </summary>
    public virtual DateTime ExecutionTime { get; set; }
    
    /// <summary>
    ///     获取或设置 执行耗时(毫秒)
    /// </summary>
    public virtual long ExecutionDuration { set; get; }
    
    /// <summary>
    ///     获取或设置 执行结果
    /// </summary>
    public virtual string ExecutionResult { set; get; }

    /// <summary>
    ///     获取或设置 操作实体属性集合
    /// </summary>
    public virtual ICollection<AuditEntityPropertyInfo> PropertyEntries { get; set; } = new List<AuditEntityPropertyInfo>();
}
