using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Findx.Data;

/// <summary>
///     审计哈希工具（GxP / 21 CFR Part 11 完整性：SHA256 哈希链）
/// </summary>
public static class AuditHashUtility
{
    /// <summary>
    ///     规范化字段值：null 统一为空字符串，保证同内容哈希稳定
    /// </summary>
    public static string Normalize(string value)
    {
        return value ?? string.Empty;
    }

    /// <summary>
    ///     计算审计记录哈希：SHA256(头部规范化内容 + 实体明细摘要 + Sql明细摘要 + 上一条哈希)
    ///     明细摘要参与哈希，明细被篡改即导致父记录哈希失配；上一条哈希参与，形成防篡改链
    /// </summary>
    /// <param name="headFields">操作头字段（顺序固定，null 自动规范化）</param>
    /// <param name="entityRows">实体明细行（顺序无关，内部自动排序）</param>
    /// <param name="sqlRows">Sql明细行（顺序无关，内部自动排序）</param>
    /// <param name="prevHash">上一条审计哈希（链首传 null 或空串）</param>
    /// <returns></returns>
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
    ///     明细行摘要：行级去空、按 Ordinal 排序后整体 SHA256
    /// </summary>
    public static string ComputeSummary(IEnumerable<string> rows)
    {
        var joined = string.Join("\n", (rows ?? Array.Empty<string>())
            .Where(r => !string.IsNullOrEmpty(r))
            .OrderBy(r => r, StringComparer.Ordinal));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(joined)));
    }
}
