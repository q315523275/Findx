using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Findx.Data;

namespace Findx.Extensions.AuditLogs.Models;

/// <summary>
///     审计日志实体属性变更信息
/// </summary>
[Table("FindxEntityPropertyChanges")]
[EntityExtension(DataSource = "AuditLog", DisableAuditing = true)]
[Description("审计操作信息")]
public class AuditEntityPropertyInfo: EntityBase<Guid>
{
    /// <summary>
    ///     获取或设置 实体主键值
    /// </summary>
    public Guid EntityChangeId { get; set; }
 
    /// <summary>
    ///     获取或设置 实体主键值
    /// </summary>
    public string EntityId { get; set; }
    
    /// <summary>
    ///     获取或设置 实体名称
    /// </summary>
    public string EntityTypeName { get; set; }
    
    /// <summary>
    ///     获取或设置 名称
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    ///     获取或设置 属性名
    /// </summary>
    public string PropertyName { get; set; }
    
    /// <summary>
    ///     获取或设置 数据类型
    /// </summary>
    public string PropertyTypeFullName { get; set; }

    /// <summary>
    ///     获取或设置 新值
    /// </summary>
    public string NewValue { get; set; }

    /// <summary>
    ///     获取或设置 旧值
    /// </summary>
    public string OriginalValue { get; set; }
}
