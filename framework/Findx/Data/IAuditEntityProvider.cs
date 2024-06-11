namespace Findx.Data;

/// <summary>
///     数据实体审计信息提供者
/// </summary>
public interface IAuditEntityProvider
{
    /// <summary>
    ///     获取数据审计信息
    /// </summary>
    /// <returns>数据审计信息的集合</returns>
    IEnumerable<AuditEntityEntry> GetAuditEntities();
}