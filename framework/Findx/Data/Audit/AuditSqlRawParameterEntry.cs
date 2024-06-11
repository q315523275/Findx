namespace Findx.Data;

/// <summary>
///     执行Sql参数审计信息
/// </summary>
public class AuditSqlRawParameterEntry
{
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