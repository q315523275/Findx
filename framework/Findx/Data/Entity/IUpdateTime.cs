namespace Findx.Data;

/// <summary>
///     定义实体更新时间
/// </summary>
public interface IUpdateTime
{
    /// <summary>
    ///     更新时间
    /// </summary>
    DateTime? LastUpdatedTime { get; set; }
}