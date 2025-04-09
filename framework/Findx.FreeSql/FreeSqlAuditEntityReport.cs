using System;
using System.Collections.Generic;
using System.Linq;
using Findx.Data;
using Findx.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.FreeSql;

/// <summary>
///     实体审计上报服务
/// </summary>
public class FreeSqlAuditEntityReport: IAuditEntityReport
{
    /// <summary>
    ///     实体审计上报
    /// </summary>
    /// <param name="auditEntityEntry"></param>
    public void AuditEntity(AuditEntityEntry auditEntityEntry)
    {
        // Todo 当前弱实现,可以增加实体变更追踪(ChangeTracker)实现全量跟踪
        // FreeSql执行前后无关联属性
        
        // var scopedDictionary = ServiceLocator.GetService<ScopedDictionary>();
        // if (scopedDictionary?.Function == null || scopedDictionary.AuditOperation?.EntityEntries == null)
        // {
        //     return;
        // }
        // scopedDictionary.AuditOperation.EntityEntries.Add(auditEntityEntry);
    }

    /// <summary>
    ///     实体审计上报
    /// </summary>
    /// <param name="auditEntityEntries"></param>
    public void AuditEntity(ICollection<AuditEntityEntry> auditEntityEntries)
    {
        // Todo 当前弱实现,可以增加实体变更追踪(ChangeTracker)实现全量跟踪
        // FreeSql执行前后无关联属性
        
        // var scopedDictionary = ServiceLocator.GetService<ScopedDictionary>();
        // if (scopedDictionary?.Function == null || scopedDictionary.AuditOperation?.EntityEntries == null)
        // {
        //     return;
        // }
        // foreach (var auditEntityEntry in auditEntityEntries)
        // {
        //     scopedDictionary.AuditOperation.EntityEntries.Add(auditEntityEntry);
        // }
    }

    /// <summary>
    ///     实体值审计上报
    /// </summary>
    /// <param name="auditEntityPropertyEntry"></param>
    public void AuditEntityProperty(AuditEntityPropertyEntry auditEntityPropertyEntry)
    {
        // 作用域字典,当前只实现Http请求关联审计信息
        var scopedDictionary = ServiceLocator.GetService<ScopedDictionary>();
        if (scopedDictionary?.Function == null || scopedDictionary.AuditOperation == null)
        {
            return;
        }
        // Todo 无追踪模块,无法精准定位
        // 关联
        var entity = scopedDictionary.AuditOperation.EntityEntries.FirstOrDefault(x => x.EntityTypeName == auditEntityPropertyEntry.EntityTypeName);
        if (entity == null)
        {
            entity = new AuditEntityEntry { EntityId = auditEntityPropertyEntry.EntityId, EntityTypeName = auditEntityPropertyEntry.EntityTypeName, ExecutionTime = DateTime.Now };
            scopedDictionary.AuditOperation.EntityEntries.Add(entity);
        }
        entity.PropertyEntries.Add(auditEntityPropertyEntry);
    }

    /// <summary>
    ///     实体值审计上报
    /// </summary>
    /// <param name="auditEntityPropertyEntries"></param>
    public void AuditEntityProperty(ICollection<AuditEntityPropertyEntry> auditEntityPropertyEntries)
    {
        // 作用域字典,当前只实现Http请求关联审计信息
        var scopedDictionary = ServiceLocator.GetService<ScopedDictionary>();
        if (scopedDictionary?.Function == null || scopedDictionary.AuditOperation == null || !scopedDictionary.AuditOperation.EntityEntries.Any())
        {
            return;
        }
        // Todo 无追踪模块,无法精准定位
        // 关联
        var auditEntityFirst = auditEntityPropertyEntries.First();
        var entity = scopedDictionary.AuditOperation.EntityEntries.FirstOrDefault(x => x.EntityTypeName == auditEntityFirst.EntityTypeName);
        if (entity == null)
        {
            entity = new AuditEntityEntry { EntityId = auditEntityFirst.EntityId, EntityTypeName = auditEntityFirst.EntityTypeName, ExecutionTime = DateTime.Now };
            scopedDictionary.AuditOperation.EntityEntries.Add(entity);
        }
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
        // 作用域字典,当前只实现Http请求关联审计信息
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
        // 作用域字典,当前只实现Http请求关联审计信息
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