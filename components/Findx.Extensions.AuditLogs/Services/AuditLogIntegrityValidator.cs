using Findx.Data;
using Findx.Extensions.AuditLogs.Models;
using Microsoft.Extensions.Logging;

namespace Findx.Extensions.AuditLogs.Services;

/// <summary>
///     AuditLogs 审计完整性校验：按 Id 升序重算每条记录哈希并与库中值比对、校验链衔接
/// </summary>
public class AuditLogIntegrityValidator
{
    private readonly IRepository<AuditLogInfo, long> _auditLogRepo;
    private readonly IRepository<AuditEntityInfo, long> _entityRepo;
    private readonly IRepository<AuditEntityPropertyInfo, long> _propertyRepo;
    private readonly IRepository<AuditSqlRawInfo, long> _sqlRepo;
    private readonly ILogger<AuditLogIntegrityValidator> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public AuditLogIntegrityValidator(
        IRepository<AuditLogInfo, long> auditLogRepo,
        IRepository<AuditEntityInfo, long> entityRepo,
        IRepository<AuditEntityPropertyInfo, long> propertyRepo,
        IRepository<AuditSqlRawInfo, long> sqlRepo,
        ILogger<AuditLogIntegrityValidator> logger)
    {
        _auditLogRepo = auditLogRepo;
        _entityRepo = entityRepo;
        _propertyRepo = propertyRepo;
        _sqlRepo = sqlRepo;
        _logger = logger;
    }

    /// <summary>
    ///     校验全链
    /// </summary>
    public async Task<AuditLogIntegrityValidationResult> ValidateAsync(CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogRepo.SelectAsync(null, new[] { new SortCondition<AuditLogInfo>("Id") }, cancellationToken);

        var result = new AuditLogIntegrityValidationResult { IsValid = true, TotalCount = logs.Count, Message = "校验通过" };
        string prevHash = null;

        foreach (var log in logs)
        {
            // 链首记录 PrevAuditHash 为空；后续必须等于前一条哈希
            if (log.PrevAuditHash != prevHash)
            {
                result.IsValid = false;
                result.BrokenAtId = log.Id;
                result.Message = $"记录 {log.Id} 链衔接断裂（期望 {prevHash ?? "空"}，实际 {log.PrevAuditHash ?? "空"}）";
                _logger.LogWarning("审计链断裂: {Message}", result.Message);
                return result;
            }

            if (string.IsNullOrEmpty(log.AuditHash))
            {
                result.IsValid = false;
                result.BrokenAtId = log.Id;
                result.Message = $"记录 {log.Id} 缺少哈希（完整性启用前写入或哈希被清空）";
                _logger.LogWarning("审计哈希缺失: {Message}", result.Message);
                return result;
            }

            var entities = await _entityRepo.SelectAsync(x => x.AuditLogId == log.Id, null, cancellationToken);
            var entityIds = entities.Select(e => e.Id).ToArray();
            var properties = entityIds.Length > 0
                ? await _propertyRepo.SelectAsync(p => entityIds.Contains(p.EntityChangeId), null, cancellationToken)
                : new List<AuditEntityPropertyInfo>();
            var sqls = await _sqlRepo.SelectAsync(x => x.AuditLogId == log.Id, null, cancellationToken);

            var expected = AuditLogIntegrityService.Compute(
                AuditLogIntegrityService.HeadFields(log),
                AuditLogIntegrityService.EntityRows(entities, properties),
                AuditLogIntegrityService.SqlRows(sqls),
                prevHash);

            if (!string.Equals(expected, log.AuditHash, StringComparison.Ordinal))
            {
                result.IsValid = false;
                result.BrokenAtId = log.Id;
                result.Message = $"记录 {log.Id} 内容哈希失配（内容被篡改）";
                _logger.LogWarning("审计哈希失配: {Message}", result.Message);
                return result;
            }

            prevHash = log.AuditHash;
        }

        return result;
    }
}
