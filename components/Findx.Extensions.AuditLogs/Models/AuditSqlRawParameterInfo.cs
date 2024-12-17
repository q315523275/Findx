using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Findx.Data;

namespace Findx.Extensions.AuditLogs.Models;

/// <summary>
///     执行Sql参数审计信息
/// </summary>
[Table("FindxSqlRawParameters")]
[EntityExtension(DataSource = "AuditLog", DisableAuditing = true)]
[Description("审计操作信息")]
public class AuditSqlRawParameterInfo: EntityBase<long>
{
    /// <summary>
    ///     日志编号
    /// </summary>
    public long SqlRawId { get; set; }
    
    /// <summary>
    ///     获取或设置 参数名
    /// </summary>
    public string ParameterName { get; set; }
    
    /// <summary>
    ///     原始列
    /// </summary>
    public string SourceColumn { get; set; }
    
    /// <summary>
    ///     获取或设置 数据类型
    /// </summary>
    public string DbType { get; set; }

    /// <summary>
    ///     获取或设置 新值
    /// </summary>
    public string Value { get; set; }
}