using Findx.Data;

namespace Findx.Extensions.AuditLogs.Models;

/// <summary>
///     审计日志方法信息
/// </summary>
public class AuditLogActionInfo: EntityBase<Guid>
{
    /// <summary>
    ///     审计Id
    /// </summary>
    public Guid? AuditLogId { set; get; }
    
    /// <summary>
    ///     租户Id
    /// </summary>
    public string TenantId { set; get; }
    
    
    /// <summary>
    ///     服务名称
    /// </summary>
    public string ServiceName { set; get; }
    
    /// <summary>
    ///     方法名称
    /// </summary>
    public string MethodName { set; get; }
    

    /// <summary>
    ///     参数信息
    /// </summary>
    public string Parameters { set; get; }
    
    /// <summary>
    ///     返回信息
    /// </summary>
    public string Result { set; get; }
    
    
    /// <summary>
    ///     执行时间
    /// </summary>
    public DateTime ExecutionTime { set; get; }
    
    /// <summary>
    ///     执行耗时(毫秒)
    /// </summary>
    public long ExecutionDuration { set; get; }
}
