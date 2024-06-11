using Findx.Messaging;

namespace Findx.Data;

/// <summary>
///     实体审计事件
/// </summary>
public class AuditEntityEvent: IApplicationEvent
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="auditEntities"></param>
    public AuditEntityEvent(IEnumerable<AuditEntityEntry> auditEntities)
    {
        AuditEntities = auditEntities;
    }

    /// <summary>
    ///     获取或设置 AuditData数据集合
    /// </summary>
    public IEnumerable<AuditEntityEntry> AuditEntities { get; }
}