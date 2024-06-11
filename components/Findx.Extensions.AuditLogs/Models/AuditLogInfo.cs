using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Findx.Data;

namespace Findx.Extensions.AuditLogs.Models;

/// <summary>
///     审计日志信息
/// </summary>
[Table("FindxAuditLogs")]
[EntityExtension(DataSource = "AuditLog")]
[Description("审计操作信息")]
public class AuditLogInfo: EntityBase<Guid>
{
    /// <summary>
    ///     应用名称
    /// </summary>
    public string ApplicationName { set; get; }
    
    
    /// <summary>
    ///     用户Id
    /// </summary>
    public string UserId { set; get; }
    
    /// <summary>
    ///     用户账号
    /// </summary>
    public string UserName { set; get; }
    
    /// <summary>
    ///     用户昵称
    /// </summary>
    public string Nickname { set; get; }
    
    
    /// <summary>
    ///     租户Id
    /// </summary>
    public string TenantId { set; get; }
    
    /// <summary>
    ///     租户名称
    /// </summary>
    public string TenantName { set; get; }
    
    
    /// <summary>
    ///     执行时间
    /// </summary>
    public DateTime ExecutionTime { set; get; }
    
    /// <summary>
    ///     执行耗时(毫秒)
    /// </summary>
    public long ExecutionDuration { set; get; }
    
    
    /// <summary>
    ///     客户端Ip
    /// </summary>
    public string ClientIpAddress { set; get; }
    
    /// <summary>
    ///     客户端名称
    /// </summary>
    public string ClientName { set; get; }
    
    /// <summary>
    ///     客户端Id
    /// </summary>
    public string ClientId { set; get; }
    
    
    /// <summary>
    ///     关联Id
    /// </summary>
    public string CorrelationId { set; get; }
    
    /// <summary>
    ///     浏览器信息
    /// </summary>
    public string BrowserInfo { set; get; }
    
    
    /// <summary>
    ///     Http请求方法
    /// </summary>
    public string HttpMethod { set; get; }
    
    /// <summary>
    ///     Url地址
    /// </summary>
    public string Url { set; get; }
 
    /// <summary>
    ///     异常信息
    /// </summary>
    public string Exceptions { set; get; }
    
    /// <summary>
    ///     Http状态码
    /// </summary>
    public int HttpStatusCode { set; get; }
    
    /// <summary>
    ///     方法名称
    /// </summary>
    public string FunctionName { set; get; }
    
    /// <summary>
    ///     备注
    /// </summary>
    public string Comments { set; get; }
    
    
    /// <summary>
    ///     获取或设置 审计实体信息集合
    /// </summary>
    public virtual ICollection<AuditEntityInfo> AuditEntities { get; set; } = new List<AuditEntityInfo>();
}
