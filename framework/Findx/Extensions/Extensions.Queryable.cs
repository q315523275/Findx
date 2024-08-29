using System.ComponentModel;
using System.Linq.Expressions;
using Findx.Common;
using Findx.Expressions;

namespace Findx.Extensions;

/// <summary>
///     系统扩展 - IQueryable
/// </summary>
public static partial class Extensions
{
    /// <summary>
    ///     分页
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="skipCount">跳过数</param>
    /// <param name="takeCount">取数</param>
    /// <returns></returns>
    public static IQueryable<T> PageBy<T>(this IQueryable<T> query, int skipCount, int takeCount)
    {
        Check.NotNull(query, nameof(query));

        return query.Skip(skipCount).Take(takeCount);
    }

    /// <summary>
    ///     分页
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TQueryable"></typeparam>
    /// <param name="query"></param>
    /// <param name="skipCount">跳过数</param>
    /// <param name="takeCount">取数</param>
    /// <returns></returns>
    public static TQueryable PageBy<T, TQueryable>(this TQueryable query, int skipCount, int takeCount) where TQueryable : IQueryable<T>
    {
        Check.NotNull(query, nameof(query));

        return (TQueryable)query.Skip(skipCount).Take(takeCount);
    }

    /// <summary>
    ///     条件通过则进行谓词过滤
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="condition"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
    {
        Check.NotNull(query, nameof(query));

        return condition ? query.Where(predicate) : query;
    }

    /// <summary>
    ///     条件通过则进行谓词过滤
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TQueryable"></typeparam>
    /// <param name="query"></param>
    /// <param name="condition"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static TQueryable WhereIf<T, TQueryable>(this TQueryable query, bool condition, Expression<Func<T, bool>> predicate) where TQueryable : IQueryable<T>
    {
        Check.NotNull(query, nameof(query));
        return condition ? (TQueryable)query.Where(predicate) : query;
    }

    /// <summary>
    ///     条件通过则进行谓词过滤
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="condition"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, int, bool>> predicate)
    {
        Check.NotNull(query, nameof(query));

        return condition ? query.Where(predicate) : query;
    }

    /// <summary>
    ///     条件通过则进行谓词过滤
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TQueryable"></typeparam>
    /// <param name="query"></param>
    /// <param name="condition"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static TQueryable WhereIf<T, TQueryable>(this TQueryable query, bool condition, Expression<Func<T, int, bool>> predicate) where TQueryable : IQueryable<T>
    {
        Check.NotNull(query, nameof(query));
        return condition ? (TQueryable)query.Where(predicate) : query;
    }
    
    
    /// <summary>
    ///     按指定的属性名称对<see cref="IEnumerable{T}"/>序列进行排序
    /// </summary>
    /// <param name="source"><see cref="IEnumerable{T}"/>序列</param>
    /// <param name="propertyName">属性名称</param>
    /// <param name="sortDirection">排序方向</param>
    public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, string propertyName, ListSortDirection sortDirection)
    {
        Check.NotNullOrWhiteSpace(propertyName, nameof(propertyName));
        dynamic expression = CollectionPropertySorter<T>.GetKeySelector(propertyName);
        var keySelector = expression.Compile();
        return sortDirection == ListSortDirection.Ascending ? Enumerable.OrderBy(source, keySelector) : Enumerable.OrderByDescending(source, keySelector);
    }

    /// <summary>
    ///     按指定的属性名称对<see cref="IOrderedEnumerable{T}"/>进行继续排序
    /// </summary>
    /// <param name="source"><see cref="IOrderedEnumerable{T}"/>序列</param>
    /// <param name="propertyName">属性名称</param>
    /// <param name="sortDirection">排序方向</param>
    public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, string propertyName, ListSortDirection sortDirection)
    {
        Check.NotNullOrWhiteSpace(propertyName, nameof(propertyName));
        dynamic expression = CollectionPropertySorter<T>.GetKeySelector(propertyName);
        var keySelector = expression.Compile();
        return sortDirection == ListSortDirection.Ascending ? Enumerable.ThenBy(source, keySelector) : Enumerable.ThenByDescending(source, keySelector);
    }

    /// <summary>
    ///     按指定的属性名称对<see cref="IQueryable{T}"/>序列进行排序
    /// </summary>
    /// <param name="source">IQueryable{T}序列</param>
    /// <param name="propertyName">属性名称</param>
    /// <param name="sortDirection">排序方向</param>
    /// <returns></returns>
    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName, ListSortDirection sortDirection)
    {
        Check.NotNullOrWhiteSpace(propertyName, nameof(propertyName));
        dynamic keySelector = CollectionPropertySorter<T>.GetKeySelector(propertyName);
        return sortDirection == ListSortDirection.Ascending ? Queryable.OrderBy(source, keySelector) : Queryable.OrderByDescending(source, keySelector);
    }

    /// <summary>
    ///     按指定的属性名称对<see cref="IOrderedQueryable{T}"/>序列进行排序
    /// </summary>
    /// <param name="source">IOrderedQueryable{T}序列</param>
    /// <param name="propertyName">属性名称</param>
    /// <param name="sortDirection">排序方向</param>
    /// <returns></returns>
    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string propertyName, ListSortDirection sortDirection)
    {
        Check.NotNullOrWhiteSpace(propertyName, nameof(propertyName));
        dynamic keySelector = CollectionPropertySorter<T>.GetKeySelector(propertyName);
        return sortDirection == ListSortDirection.Ascending ? Queryable.ThenBy(source, keySelector) : Queryable.ThenByDescending(source, keySelector);
    }

    // /// <summary>
    // ///     根据指定的排序条件进行集合排序
    // /// </summary>
    // /// <param name="query"></param>
    // /// <param name="sortConditions"></param>
    // /// <typeparam name="T"></typeparam>
    // /// <returns></returns>
    // public static IOrderedQueryable<T> OrderConditions<T>(this IQueryable<T> query, IEnumerable<SortCondition> sortConditions)
    // {
    //     var parameter = Expression.Parameter(typeof(T), "x");
    //     foreach (var orderInfo in sortConditions)
    //     {
    //         Expression propertyAccess = parameter;
    //         var propertyNames = orderInfo.SortField.Split('.');
    //         if (propertyNames.Length > 1)
    //         {
    //             var type = parameter.Type;
    //             foreach (var propertyName in propertyNames)
    //             {
    //                 var property = type.GetProperty(propertyName);
    //                 if (property == null)
    //                     throw new InvalidOperationException($"指定对象中不存在名称为“{propertyName}”的属性。");
    //                 // 子对象类型
    //                 type = property.PropertyType;
    //                 propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
    //             }
    //         }
    //         else
    //         {
    //             propertyAccess = Expression.Property(parameter, orderInfo.SortField);
    //         }
    //
    //         if (propertyAccess.Type == typeof(long))
    //         {
    //             var orderBy = Expression.Lambda<Func<T, long>>(propertyAccess, parameter);
    //             query = orderInfo.ListSortDirection == ListSortDirection.Descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
    //         }
    //         else if (propertyAccess.Type == typeof(int))
    //         {
    //             var orderBy = Expression.Lambda<Func<T, int>>(propertyAccess, parameter);
    //             query = orderInfo.ListSortDirection == ListSortDirection.Descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
    //         } 
    //         else if (propertyAccess.Type == typeof(TimeSpan))
    //         {
    //             var orderBy = Expression.Lambda<Func<T, TimeSpan>>(propertyAccess, parameter);
    //             query = orderInfo.ListSortDirection == ListSortDirection.Descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
    //         }
    //         else if (propertyAccess.Type == typeof(DateTime))
    //         {
    //             var orderBy = Expression.Lambda<Func<T, DateTime>>(propertyAccess, parameter);
    //             query = orderInfo.ListSortDirection == ListSortDirection.Descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
    //         }
    //         else if (propertyAccess.Type == typeof(DateTime?))
    //         {
    //             var orderBy = Expression.Lambda<Func<T, DateTime?>>(propertyAccess, parameter);
    //             query = orderInfo.ListSortDirection == ListSortDirection.Descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
    //         }
    //         else
    //         {
    //             var orderBy = Expression.Lambda<Func<T, object>>(propertyAccess, parameter);
    //             query = orderInfo.ListSortDirection == ListSortDirection.Descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
    //         }
    //     }
    //     return query;
    // }
}