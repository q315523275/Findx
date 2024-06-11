namespace Findx.Data;

/// <summary>
///     实体审计信息
/// </summary>
public class AuditEntityEntry
{
    /// <summary>
    ///     获取或设置 数据编号
    /// </summary>
    public string EntityId { get; set; }
    
    /// <summary>
    ///     获取或设置 实体名称
    /// </summary>
    public string EntityTypeName { get; set; }
    
    /// <summary>
    ///     获取或设置 实体全称
    /// </summary>
    public string EntityTypeFullName { get; set; }

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
    /// 获取或设置 操作实体属性集合
    /// </summary>
    public ICollection<AuditEntityPropertyEntry> PropertyEntries { get; set; } = [];
}