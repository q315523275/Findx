using Findx.Data;
using Findx.Extensions.AuditLogs.Events;
using Findx.Extensions.AuditLogs.Models;
using Findx.Messaging;

namespace Findx.Extensions.AuditLogs.EventHandling;

public class AuditLogSaveEventHandler: IApplicationEventHandler<AuditLogSaveEvent>
{
    private readonly IRepository<AuditLogInfo> _auditLogRepo;
    private readonly IRepository<AuditEntityInfo> _auditEntityRepo;
    private readonly IRepository<AuditEntityPropertyInfo> _auditEntityPropertyRepo;
    private readonly IRepository<AuditSqlRawInfo> _auditSqlRawRepo;
    private readonly IRepository<AuditSqlRawParameterInfo> _auditSqlRawParameterRepo;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="auditLogRepo"></param>
    /// <param name="auditEntityRepo"></param>
    /// <param name="auditEntityPropertyRepo"></param>
    /// <param name="auditSqlRawRepo"></param>
    /// <param name="auditSqlRawParameterRepo"></param>
    public AuditLogSaveEventHandler(IRepository<AuditLogInfo> auditLogRepo, IRepository<AuditEntityInfo> auditEntityRepo, IRepository<AuditEntityPropertyInfo> auditEntityPropertyRepo, IRepository<AuditSqlRawInfo> auditSqlRawRepo, IRepository<AuditSqlRawParameterInfo> auditSqlRawParameterRepo)
    {
        _auditLogRepo = auditLogRepo;
        _auditEntityRepo = auditEntityRepo;
        _auditEntityPropertyRepo = auditEntityPropertyRepo;
        _auditSqlRawRepo = auditSqlRawRepo;
        _auditSqlRawParameterRepo = auditSqlRawParameterRepo;
    }

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