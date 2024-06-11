namespace Findx.Data;

/// <summary>
///     实体审计上报服务
/// </summary>
public interface IAuditEntityReport
{
    /// <summary>
    ///     实体审计上报
    /// </summary>
    /// <param name="auditEntityEntry"></param>
    void AuditEntity(AuditEntityEntry auditEntityEntry);
    
    /// <summary>
    ///     实体审计上报
    /// </summary>
    /// <param name="auditEntityEntries"></param>
    void AuditEntity(ICollection<AuditEntityEntry> auditEntityEntries);
    
    /// <summary>
    ///     实体值审计上报
    /// </summary>
    /// <param name="auditEntityPropertyEntry"></param>
    void AuditEntityProperty(AuditEntityPropertyEntry auditEntityPropertyEntry);
    
    /// <summary>
    ///     实体值审计上报
    /// </summary>
    /// <param name="auditEntityPropertyEntries"></param>
    void AuditEntityProperty(ICollection<AuditEntityPropertyEntry> auditEntityPropertyEntries);
    
    /// <summary>
    ///     Sql执行审计上报
    /// </summary>
    /// <param name="auditSqlRawEntry"></param>
    void AuditSqlRaw(AuditSqlRawEntry auditSqlRawEntry);
    
    /// <summary>
    ///     Sql执行审计上报
    /// </summary>
    /// <param name="auditSqlRawEntries"></param>
    void AuditSqlRaw(ICollection<AuditSqlRawEntry> auditSqlRawEntries);
}