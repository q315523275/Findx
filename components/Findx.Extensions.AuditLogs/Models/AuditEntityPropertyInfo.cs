using Findx.Data;

namespace Findx.Extensions.AuditLogs.Models;

/// <summary>
///     审计日志实体属性变更信息
/// </summary>
public class AuditEntityPropertyInfo: EntityBase<Guid>
{
    /// <summary>
    ///     租户Id
    /// </summary>
    public string TenantId { set; get; }
    
    /// <summary>
    ///     实体变更Id
    /// </summary>
    public Guid? EntityChangeId { set; get; }
   
    
    /// <summary>
    ///     变更值
    /// </summary>
    public string NewValue { set; get; }
    
    /// <summary>
    ///     变更前原始值
    /// </summary>
    public string OriginalValue { set; get; }

    
    /// <summary>
    ///     属性名称
    /// </summary>
    public string PropertyName { set; get; }
    
    /// <summary>
    ///     属性命名空间
    /// </summary>
    public string PropertyTypeFullName { set; get; }
}
