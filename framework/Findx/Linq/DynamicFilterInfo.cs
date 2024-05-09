namespace Findx.Linq;

/// <summary>
///     动态筛选规则
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class DynamicFilterInfo
{
    /// <summary>
    ///     筛选集合组合方式
    /// </summary>
    public FilterOperate Logic { get; set; } = FilterOperate.And;
    
    /// <summary>
    ///     筛选器集合
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<FilterConditions> Filters { get; set; } = [];
}