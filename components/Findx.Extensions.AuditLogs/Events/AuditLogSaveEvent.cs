using Findx.Common;
using Findx.Extensions.AuditLogs.Models;
using Findx.Messaging;

namespace Findx.Extensions.AuditLogs.Events;

/// <summary>
///     审计日志保存事件
/// </summary>
public class AuditLogSaveEvent: IApplicationEvent, IAsync
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="auditLogInfo"></param>
    public AuditLogSaveEvent(AuditLogInfo auditLogInfo)
    {
        AuditLogInfo = auditLogInfo;
    }

    /// <summary>
    ///     审计日志
    /// </summary>
    public AuditLogInfo AuditLogInfo { get; set; }
}