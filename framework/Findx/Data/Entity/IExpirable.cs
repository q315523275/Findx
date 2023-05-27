namespace Findx.Data;

/// <summary>
///     定义实体有效期字段
/// </summary>
public interface IExpirable
{
    /// <summary>
    ///     获取或设置 生效时间
    /// </summary>
    DateTime? BeginTime { get; set; }

    /// <summary>
    ///     获取或设置 过期时间
    /// </summary>
    DateTime? EndTime { get; set; }
}