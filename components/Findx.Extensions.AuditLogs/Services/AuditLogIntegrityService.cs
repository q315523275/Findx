using System.Security.Cryptography;
using System.Text;
using Findx.Extensions.AuditLogs.Models;

namespace Findx.Extensions.AuditLogs.Services;

/// <summary>
///     审计哈希计算（SHA256 哈希链）。算法与主框架 Findx.AuditHashUtility 一致；
///     本模块经 NuGet 依赖旧版 Findx 包暂自包含等价实现，Findx 发版后应对齐切换
/// </summary>
public static class AuditLogIntegrityService
{
    /// <summary>
    ///     计算审计记录哈希：SHA256(头部规范化内容 + 实体变更摘要 + Sql摘要 + 上一条哈希)
    /// </summary>
    public static string Compute(IEnumerable<string> headFields, IEnumerable<string> entityRows, IEnumerable<string> sqlRows, string prevHash)
    {
        var head = string.Join("\n", (headFields ?? Array.Empty<string>()).Select(Normalize));
        var entitySummary = ComputeSummary(entityRows);
        var sqlSummary = ComputeSummary(sqlRows);
        var canonical = head + "\n" + entitySummary + "\n" + sqlSummary;
        var payload = canonical + "|" + Normalize(prevHash);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    }

    /// <summary>
    ///     规范化字段值：null 统一为空字符串
    /// </summary>
    public static string Normalize(string value)
    {
        return value ?? string.Empty;
    }

    /// <summary>
    ///     明细行摘要：行级去空、按 Ordinal 排序后整体 SHA256
    /// </summary>
    public static string ComputeSummary(IEnumerable<string> rows)
    {
        var joined = string.Join("\n", (rows ?? Array.Empty<string>())
            .Where(r => !string.IsNullOrEmpty(r))
            .OrderBy(r => r, StringComparer.Ordinal));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(joined)));
    }

    /// <summary>
    ///     操作头规范化字段（顺序固定，参与哈希）
    /// </summary>
    public static string[] HeadFields(AuditLogInfo log)
    {
        return new[]
        {
            log.ApplicationName, log.UserId, log.Username, log.Nickname, log.TenantId, log.TenantName,
            log.ExecutionTime.ToString("o"), log.ExecutionDuration.ToString(),
            log.ClientIpAddress, log.ClientName, log.ClientId, log.CorrelationId, log.BrowserInfo,
            log.HttpMethod, log.Url, log.Exceptions, log.HttpStatusCode.ToString(), log.FunctionName, log.Comments
        };
    }

    /// <summary>
    ///     实体变更行（属性挂在实体集合上的落库路径）
    /// </summary>
    public static IEnumerable<string> EntityRows(IEnumerable<AuditEntityInfo> entities)
    {
        return EntityRows(entities, entities.SelectMany(e => e.PropertyEntries));
    }

    /// <summary>
    ///     实体变更行（属性独立查询的校验路径，按 EntityChangeId 归并，与落库路径行构造一致）
    /// </summary>
    public static IEnumerable<string> EntityRows(IEnumerable<AuditEntityInfo> entities, IEnumerable<AuditEntityPropertyInfo> properties)
    {
        var propertyByEntity = properties
            .GroupBy(p => p.EntityChangeId)
            .ToDictionary(g => g.Key, g => g.ToArray());

        foreach (var entity in entities)
        {
            if (propertyByEntity.TryGetValue(entity.Id, out var props))
            {
                foreach (var property in props)
                {
                    yield return string.Join("|", entity.EntityTypeFullName, property.EntityId ?? entity.EntityId,
                        property.PropertyName, property.OriginalValue, property.NewValue, entity.ExecutionResult);
                }
            }
        }
    }

    /// <summary>
    ///     Sql明细行（参与哈希，自动排序）
    /// </summary>
    public static IEnumerable<string> SqlRows(IEnumerable<AuditSqlRawInfo> sqls)
    {
        return sqls.Select(s => string.Join("|", s.DbTableName, s.SqlRaw, s.ExecutionResult));
    }
}
