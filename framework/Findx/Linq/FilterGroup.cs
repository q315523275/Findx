namespace Findx.Linq;

/// <summary>
///     动态筛选规则
/// </summary>
public class FilterGroup
{
    /// <summary>
    ///     筛选集合组合方式
    /// </summary>
    public FilterOperate Logic { get; set; } = FilterOperate.And;
    
    /// <summary>
    ///     筛选器集合
    /// </summary>
    public IEnumerable<FilterCondition> Filters { get; set; } = new List<FilterCondition>();
}