using System.ComponentModel;
using System.Linq.Expressions;
using Findx.Linq;

namespace Findx.Data;

/// <summary>
///     排序参数
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class OrderByParameter<TEntity>
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public OrderByParameter()
    {
    }
    
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="field"></param>
    /// <param name="sortDirection"></param>
    public OrderByParameter(string field, ListSortDirection sortDirection = ListSortDirection.Ascending)
    {
        dynamic keySelector = CollectionPropertySorter<TEntity>.GetKeySelector(field);
        Conditions = keySelector;
        SortDirection = sortDirection;
    }
    
    /// <summary>
    ///     排序字段表达式
    /// </summary>
    public Expression<Func<TEntity, object>> Conditions { set; get; }

    /// <summary>
    ///     排序方向
    /// </summary>
    public ListSortDirection SortDirection { set; get; } = ListSortDirection.Ascending;
}