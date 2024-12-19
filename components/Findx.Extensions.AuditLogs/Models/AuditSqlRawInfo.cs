using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Findx.Data;

namespace Findx.Extensions.AuditLogs.Models;

/// <summary>
///     执行Sql审计信息
/// </summary>
[Table("findx_sql_raw")]
[EntityExtension(DataSource = "AuditLog", DisableAuditing = true)]
[Description("审计操作信息")]
public class AuditSqlRawInfo: EntityBase<long>
{
    /// <summary>
    ///     日志编号
    /// </summary>
    public long AuditLogId { get; set; }
    
    /// <summary>
    ///     获取或设置 实体全称
    /// </summary>
    public string EntityTypeFullName { get; set; }

    /// <summary>
    ///     获取或设置 数据库表名
    /// </summary>
    public string DbTableName { set; get; }
    
    /// <summary>
    ///     获取或设置 执行Sql内容
    /// </summary>
    public string SqlRaw { get; set; }
    
    /// <summary>
    ///     获取或设置 执行时间
    /// </summary>
    public DateTime ExecutionTime { get; set; }
    
    /// <summary>
    ///     获取或设置 执行耗时(毫秒)
    /// </summary>
    public long ExecutionDuration { set; get; }
    
    /// <summary>
    ///     获取或设置 执行结果
    /// </summary>
    public string ExecutionResult { set; get; }

    /// <summary>
    ///     获取或设置 操作实体属性集合
    /// </summary>
    public ICollection<AuditSqlRawParameterInfo> DbParameters { get; set; } = new List<AuditSqlRawParameterInfo>();
}