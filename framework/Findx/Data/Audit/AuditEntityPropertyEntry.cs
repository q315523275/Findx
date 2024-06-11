namespace Findx.Data;

/// <summary>
///     实体属性审计信息
/// </summary>
public class AuditEntityPropertyEntry
{
    /// <summary>
    ///     获取或设置 实体主键值
    /// </summary>
    public string EntityId { get; set; }
    
    /// <summary>
    ///     获取或设置 实体名称
    /// </summary>
    public string EntityTypeName { get; set; }
    
    /// <summary>
    ///     获取或设置 名称
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    ///     获取或设置 属性名
    /// </summary>
    public string PropertyName { get; set; }
    
    /// <summary>
    ///     获取或设置 数据类型
    /// </summary>
    public string PropertyTypeFullName { get; set; }

    /// <summary>
    ///     获取或设置 新值
    /// </summary>
    public string NewValue { get; set; }

    /// <summary>
    ///     获取或设置 旧值
    /// </summary>
    public string OriginalValue { get; set; }
}