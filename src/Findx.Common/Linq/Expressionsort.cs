using System.ComponentModel;
using System.Linq.Expressions;
using Findx.Data;

namespace Findx.Linq;

/// <summary>
/// Expression表达式扩展类
/// </summary>
public class Expressionsort<T> where T : class, new()
{
    /// <summary>
    /// 排序集合
    /// </summary>
    private readonly List<OrderByParameter<T>> orderList = new();

    /// <summary>
    /// 正序
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public Expressionsort<T> OrderBy(Expression<Func<T, object>> expression)
    {
        orderList.Add(new OrderByParameter<T>() { Expression = expression, SortDirection = ListSortDirection.Ascending});
        return this;
    }

    /// <summary>
    /// 正序
    /// </summary>
    /// <param name="isExp"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    public Expressionsort<T> OrderByIF(bool isExp, Expression<Func<T, object>> expression)
    {
        if (isExp)
        {
            OrderBy(expression);
        }
        return this;
    }

    /// <summary>
    /// 倒序
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public Expressionsort<T> OrderByDescending(Expression<Func<T, object>> expression)
    {
        orderList.Add(new OrderByParameter<T>() { Expression = expression, SortDirection = ListSortDirection.Descending});
        return this;
    }

    /// <summary>
    /// 倒序
    /// </summary>
    /// <param name="isExp"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    public Expressionsort<T> OrderByDescendingIF(bool isExp, Expression<Func<T, object>> expression)
    {
        if (isExp)
        {
            OrderByDescending(expression);
        }
        return this;
    }

    /// <summary>
    /// 排序
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="sortDirection"></param>
    /// <returns></returns>
    public Expressionsort<T> Order(Expression<Func<T, object>> expression, ListSortDirection sortDirection)
    {
        switch (sortDirection)
        {
            case ListSortDirection.Ascending:
                OrderBy(expression);
                break;
            case ListSortDirection.Descending:
                OrderByDescending(expression);
                break;
        }

        return this;
    }

    /// <summary>
    /// 返回排序集合
    /// </summary>
    /// <returns></returns>
    public List<OrderByParameter<T>> ToSort()
    {
        return orderList;
    }
}