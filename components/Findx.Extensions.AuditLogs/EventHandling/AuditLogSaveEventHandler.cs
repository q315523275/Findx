using Findx.Data;
using Findx.Extensions.AuditLogs.Events;
using Findx.Extensions.AuditLogs.Models;
using Findx.Messaging;

namespace Findx.Extensions.AuditLogs.EventHandling;

/// <summary>
///     审计日志保存事件
/// </summary>
public class AuditLogSaveEventHandler: IApplicationEventHandler<AuditLogSaveEvent>
{
    private readonly IRepository<AuditLogInfo, long> _auditLogRepo;
    private readonly IRepository<AuditEntityInfo, long> _auditEntityRepo;
    private readonly IRepository<AuditEntityPropertyInfo, long> _auditEntityPropertyRepo;
    private readonly IRepository<AuditSqlRawInfo, long> _auditSqlRawRepo;
    private readonly IRepository<AuditSqlRawParameterInfo, long> _auditSqlRawParameterRepo;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="auditLogRepo"></param>
    /// <param name="auditEntityRepo"></param>
    /// <param name="auditEntityPropertyRepo"></param>
    /// <param name="auditSqlRawRepo"></param>
    /// <param name="auditSqlRawParameterRepo"></param>
    public AuditLogSaveEventHandler(IRepository<AuditLogInfo, long> auditLogRepo, IRepository<AuditEntityInfo, long> auditEntityRepo, IRepository<AuditEntityPropertyInfo, long> auditEntityPropertyRepo, IRepository<AuditSqlRawInfo, long> auditSqlRawRepo, IRepository<AuditSqlRawParameterInfo, long> auditSqlRawParameterRepo)
    {
        _auditLogRepo = auditLogRepo;
        _auditEntityRepo = auditEntityRepo;
        _auditEntityPropertyRepo = auditEntityPropertyRepo;
        _auditSqlRawRepo = auditSqlRawRepo;
        _auditSqlRawParameterRepo = auditSqlRawParameterRepo;
    }

    /// <summary>
    ///     do
    /// </summary>
    /// <param name="eventData"></param>
    /// <param name="cancellationToken"></param>
    public async Task HandleAsync(AuditLogSaveEvent eventData, CancellationToken cancellationToken = default)
    {
        if (eventData?.AuditLogInfo != null)
        {
            var auditLogInfo = eventData.AuditLogInfo;
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
}