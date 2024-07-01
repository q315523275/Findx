using System.ComponentModel;
using System.Linq.Expressions;
using Findx.Data;

namespace Findx.Linq;

/// <summary>
///     Expression表达式扩展类
/// </summary>
public class ExpressionSorter<T>
{
    /// <summary>
    ///     排序集合
    /// </summary>
    private readonly List<OrderByParameter<T>> _orderList = [];

    /// <summary>
    ///     正序
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public ExpressionSorter<T> OrderBy(Expression<Func<T, object>> expression)
    {
        _orderList.Add(new OrderByParameter<T> { Conditions = expression, SortDirection = ListSortDirection.Ascending });
        return this;
    }
    
    /// <summary>
    ///     正序
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    public ExpressionSorter<T> OrderBy(string field)
    {
        _orderList.Add(new OrderByParameter<T>(field, ListSortDirection.Ascending));
        return this;
    }

    /// <summary>
    ///     正序
    /// </summary>
    /// <param name="isExp"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    public ExpressionSorter<T> OrderByIf(bool isExp, Expression<Func<T, object>> expression)
    {
        if (isExp) OrderBy(expression);
        return this;
    }
    
    /// <summary>
    ///     正序
    /// </summary>
    /// <param name="isExp"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    public ExpressionSorter<T> OrderByIf(bool isExp, string field)
    {
        if (isExp) OrderBy(field);
        return this;
    }

    /// <summary>
    ///     倒序
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public ExpressionSorter<T> OrderByDescending(Expression<Func<T, object>> expression)
    {
        _orderList.Add(new OrderByParameter<T> { Conditions = expression, SortDirection = ListSortDirection.Descending });
        return this;
    }
    
    /// <summary>
    ///     倒序
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    public ExpressionSorter<T> OrderByDescending(string field)
    {
        _orderList.Add(new OrderByParameter<T>(field, ListSortDirection.Descending));
        return this;
    }

    /// <summary>
    ///     倒序
    /// </summary>
    /// <param name="isExp"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    public ExpressionSorter<T> OrderByDescendingIf(bool isExp, Expression<Func<T, object>> expression)
    {
        if (isExp) OrderByDescending(expression);
        return this;
    }
    
    /// <summary>
    ///     倒序
    /// </summary>
    /// <param name="isExp"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    public ExpressionSorter<T> OrderByDescendingIf(bool isExp, string field)
    {
        if (isExp) OrderByDescending(field);
        return this;
    }

    /// <summary>
    ///     排序
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="sortDirection"></param>
    /// <returns></returns>
    public ExpressionSorter<T> Order(Expression<Func<T, object>> expression, ListSortDirection sortDirection)
    {
        switch (sortDirection)
        {
            case ListSortDirection.Ascending:
                OrderBy(expression);
                break;
            
            case ListSortDirection.Descending:
                OrderByDescending(expression);
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(sortDirection), sortDirection, null);
        }
        return this;
    }
    
    /// <summary>
    ///     排序
    /// </summary>
    /// <param name="field"></param>
    /// <param name="sortDirection"></param>
    /// <returns></returns>
    public ExpressionSorter<T> Order(string field, ListSortDirection sortDirection)
    {
        switch (sortDirection)
        {
            case ListSortDirection.Ascending:
                OrderBy(field);
                break;
            
            case ListSortDirection.Descending:
                OrderByDescending(field);
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(sortDirection), sortDirection, null);
        }
        return this;
    }

    /// <summary>
    ///     排序
    /// </summary>
    /// <param name="isExp"></param>
    /// <param name="field"></param>
    /// <param name="sortDirection"></param>
    /// <returns></returns>
    public ExpressionSorter<T> OrderIf(bool isExp, string field, ListSortDirection sortDirection)
    {
        if (!isExp)
            return this;
        
        switch (sortDirection)
        {
            case ListSortDirection.Ascending:
                OrderBy(field);
                break;
            
            case ListSortDirection.Descending:
                OrderByDescending(field);
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(sortDirection), sortDirection, null);
        }
        return this;
    }

    /// <summary>
    ///     返回排序集合
    /// </summary>
    /// <returns></returns>
    public List<OrderByParameter<T>> Build()
    {
        return _orderList;
    }
}