using Findx.DependencyInjection;

namespace Findx.Data;

/// <summary>
///     实体审计上报服务
/// </summary>
public class DefaultAuditEntityReport: IAuditEntityReport
{
    /// <summary>
    ///     实体审计上报
    /// </summary>
    /// <param name="auditEntityEntry"></param>
    public void AuditEntity(AuditEntityEntry auditEntityEntry)
    {
        var scopedDictionary = ServiceLocator.GetService<ScopedDictionary>();
        if (scopedDictionary?.Function == null || scopedDictionary.AuditOperation?.EntityEntries == null)
        {
            return;
        }
        scopedDictionary.AuditOperation.EntityEntries.Add(auditEntityEntry);
    }

    /// <summary>
    ///     实体审计上报
    /// </summary>
    /// <param name="auditEntityEntries"></param>
    public void AuditEntity(ICollection<AuditEntityEntry> auditEntityEntries)
    {
        var scopedDictionary = ServiceLocator.GetService<ScopedDictionary>();
        if (scopedDictionary?.Function == null || scopedDictionary.AuditOperation?.EntityEntries == null)
        {
            return;
        }
        foreach (var auditEntityEntry in auditEntityEntries)
        {
            scopedDictionary.AuditOperation.EntityEntries.Add(auditEntityEntry);
        }
    }

    /// <summary>
    ///     实体值审计上报
    /// </summary>
    /// <param name="auditEntityPropertyEntry"></param>
    public void AuditEntityProperty(AuditEntityPropertyEntry auditEntityPropertyEntry)
    {
        // 作用域字典
        var scopedDictionary = ServiceLocator.GetService<ScopedDictionary>();
        if (scopedDictionary?.Function == null || scopedDictionary.AuditOperation == null)
        {
            return;
        }
        // 关联
        var entity = scopedDictionary.AuditOperation.EntityEntries.FirstOrDefault(x => x.EntityId == auditEntityPropertyEntry.EntityId);
        entity?.PropertyEntries.Add(auditEntityPropertyEntry);
    }

    /// <summary>
    ///     实体值审计上报
    /// </summary>
    /// <param name="auditEntityPropertyEntries"></param>
    public void AuditEntityProperty(ICollection<AuditEntityPropertyEntry> auditEntityPropertyEntries)
    {
        // 作用域字典
        var scopedDictionary = ServiceLocator.GetService<ScopedDictionary>();
        if (scopedDictionary?.Function == null || scopedDictionary.AuditOperation == null || !scopedDictionary.AuditOperation.EntityEntries.Any())
        {
            return;
        }
        // 关联
        var entityId = auditEntityPropertyEntries.First().EntityId;
        var entity = scopedDictionary.AuditOperation.EntityEntries.FirstOrDefault(x => x.EntityId == entityId);
        foreach (var auditEntityPropertyEntry in auditEntityPropertyEntries)
        {
            entity?.PropertyEntries.Add(auditEntityPropertyEntry);
        }
    }

    /// <summary>
    ///     审计Sql执行
    /// </summary>
    /// <param name="auditSqlRawEntry"></param>
    public void AuditSqlRaw(AuditSqlRawEntry auditSqlRawEntry)
    {
        var scopedDictionary = ServiceLocator.GetService<ScopedDictionary>();
        if (scopedDictionary?.Function == null || scopedDictionary.AuditOperation?.SqlRawEntries == null)
        {
            return;
        }
        scopedDictionary.AuditOperation.SqlRawEntries.Add(auditSqlRawEntry);
    }

    /// <summary>
    ///     审计Sql执行
    /// </summary>
    /// <param name="auditSqlRawEntries"></param>
    public void AuditSqlRaw(ICollection<AuditSqlRawEntry> auditSqlRawEntries)
    {
        var scopedDictionary = ServiceLocator.GetService<ScopedDictionary>();
        if (scopedDictionary?.Function == null || scopedDictionary.AuditOperation?.SqlRawEntries == null)
        {
            return;
        }
        foreach (var auditSqlRawEntry in auditSqlRawEntries)
        {
            scopedDictionary.AuditOperation.SqlRawEntries.Add(auditSqlRawEntry);
        }
    }
}