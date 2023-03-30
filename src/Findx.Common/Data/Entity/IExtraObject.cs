namespace Findx.Data;

/// <summary>
/// 定义实体扩展对象
/// </summary>
public interface IExtraObject
{
    /// <summary>
    /// 获取或设置 扩展Json数据
    /// <remarks>
    /// {
    ///   "Property1" : ...
    ///   "Property2" : ...
    /// }
    /// </remarks>
    /// </summary>
    string ExtraProperties { get; set; }
}