using System.ComponentModel;
using Findx.Data;
using Findx.Extensions.AuditLogs.Events;
using Findx.Extensions.AuditLogs.Models;
using Findx.Extensions.AuditLogs.Options;
using Findx.Extensions.AuditLogs.ServiceDefaults;
using Findx.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.AuditLogs.EventHandling;

/// <summary>
///     审计日志保存事件处理器（GxP：完整性开启时在落库前计算 SHA256 哈希链）
/// </summary>
public class AuditLogSaveEventHandler: IApplicationEventHandler<AuditLogSaveEvent>
{
    // 哈希链写入必须串行：并发行若拿到同一链尾则链条断裂（仅 EnableIntegrity 时使用）
    private static readonly SemaphoreSlim ChainLock = new(1, 1);

    private readonly IRepository<AuditLogInfo, long> _auditLogRepo;
    private readonly IRepository<AuditEntityInfo, long> _auditEntityRepo;
    private readonly IRepository<AuditEntityPropertyInfo, long> _auditEntityPropertyRepo;
    private readonly IRepository<AuditSqlRawInfo, long> _auditSqlRawRepo;
    private readonly IRepository<AuditSqlRawParameterInfo, long> _auditSqlRawParameterRepo;
    private readonly AuditLogOptions _options;
    private readonly ILogger<AuditLogSaveEventHandler> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public AuditLogSaveEventHandler(IRepository<AuditLogInfo, long> auditLogRepo, IRepository<AuditEntityInfo, long> auditEntityRepo, IRepository<AuditEntityPropertyInfo, long> auditEntityPropertyRepo, IRepository<AuditSqlRawInfo, long> auditSqlRawRepo, IRepository<AuditSqlRawParameterInfo, long> auditSqlRawParameterRepo, IOptions<AuditLogOptions> options, ILogger<AuditLogSaveEventHandler> logger)
    {
        _auditLogRepo = auditLogRepo;
        _auditEntityRepo = auditEntityRepo;
        _auditEntityPropertyRepo = auditEntityPropertyRepo;
        _auditSqlRawRepo = auditSqlRawRepo;
        _auditSqlRawParameterRepo = auditSqlRawParameterRepo;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    ///     do
    /// </summary>
    public async Task HandleAsync(AuditLogSaveEvent eventData, CancellationToken cancellationToken = default)
    {
        if (eventData?.AuditLogInfo == null)
            return;

        var auditLogInfo = eventData.AuditLogInfo;

        // GxP 完整性：链尾查询 + 哈希计算 + 主记录落库必须在同一临界区，保证链条顺序
        if (_options.EnableIntegrity)
        {
            await ChainLock.WaitAsync(cancellationToken);
            try
            {
                await AttachIntegrityHashAsync(auditLogInfo, cancellationToken);
                await InsertAllAsync(auditLogInfo, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "审计完整性落库失败: {FunctionName}", auditLogInfo.FunctionName);
                throw;
            }
            finally
            {
                ChainLock.Release();
            }
            return;
        }

        await InsertAllAsync(auditLogInfo, cancellationToken);
    }

    /// <summary>
    ///     查询链尾哈希并计算本记录哈希（须在临界区内调用）
    /// </summary>
    private async Task AttachIntegrityHashAsync(AuditLogInfo auditLogInfo, CancellationToken cancellationToken)
    {
        var last = await _auditLogRepo.TopAsync(1, x => x.AuditHash != null,
            new[] { new SortCondition<AuditLogInfo>("Id", ListSortDirection.Descending) }, cancellationToken);
        var prevHash = last.FirstOrDefault()?.AuditHash;

        auditLogInfo.PrevAuditHash = prevHash;
        auditLogInfo.AuditHash = AuditLogIntegrityService.Compute(
            AuditLogIntegrityService.HeadFields(auditLogInfo),
            AuditLogIntegrityService.EntityRows(auditLogInfo.EntityEntries),
            AuditLogIntegrityService.SqlRows(auditLogInfo.SqlRawEntries),
            prevHash);
        auditLogInfo.HashAlgorithm = _options.HashAlgorithm;
    }

    private async Task InsertAllAsync(AuditLogInfo auditLogInfo, CancellationToken cancellationToken)
    {
        await _auditLogRepo.InsertAsync(auditLogInfo, cancellationToken);
        if (auditLogInfo.EntityEntries.Any())
        {
            await _auditEntityRepo.InsertAsync(auditLogInfo.EntityEntries, cancellationToken);
            var changeList = auditLogInfo.EntityEntries.SelectMany(x => x.PropertyEntries);
            if (changeList.Any())
                await _auditEntityPropertyRepo.InsertAsync(changeList, cancellationToken);
        }

        if (auditLogInfo.SqlRawEntries.Any())
        {
            await _auditSqlRawRepo.InsertAsync(auditLogInfo.SqlRawEntries, cancellationToken);
            var paramList = auditLogInfo.SqlRawEntries.SelectMany(x => x.DbParameters);
            if (paramList.Any())
                await _auditSqlRawParameterRepo.InsertAsync(paramList, cancellationToken);
        }
    }
}
