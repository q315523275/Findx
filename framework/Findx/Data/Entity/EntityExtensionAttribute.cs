namespace Findx.Data;

/// <summary>
///     定义一个数据源配置属性
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class EntityExtensionAttribute : Attribute
{
    /// <summary>
    ///     数据源标识
    /// </summary>
    public string DataSource { get; set; }

    /// <summary>
    ///     是否包含软删除
    /// </summary>
    public bool? HasSoftDeletable { get; set; }

    /// <summary>
    ///     是否包含分表
    /// </summary>
    public bool? HasTableSharding { get; set; }
}