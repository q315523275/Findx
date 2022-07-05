using Microsoft.Extensions.Options;

namespace Findx.Data;

/// <summary>
/// 审计配置
/// </summary>
public class AuditingOptions: IOptions<AuditingOptions>
{
    /// <summary>
    /// 值
    /// </summary>
    public AuditingOptions Value => this;
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { set; get; }
    
    /// <summary>
    /// 是否记录请求参数
    /// </summary>
    public bool RecordParameters { set; get; }
}