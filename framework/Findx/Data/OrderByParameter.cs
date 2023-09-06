using System.ComponentModel;
using System.Linq.Expressions;

namespace Findx.Data;

/// <summary>
///     排序参数
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class OrderByParameter<TEntity>
{
    /// <summary>
    /// Ctor
    /// </summary>
    public OrderByParameter()
    {
    }
    
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="field"></param>
    /// <param name="sortDirection"></param>
    public OrderByParameter(string field, ListSortDirection sortDirection = ListSortDirection.Descending)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        Expression propertyAccess = parameter;
        var propertyNames = field.Split('.');
        if (propertyNames.Length > 1)
        {
            var type = parameter.Type;
            foreach (var propertyName in propertyNames)
            {
                var property = type.GetProperty(propertyName);
                if (property == null)
                    throw new InvalidOperationException($"指定的属性“{propertyName}”在类型“{type.FullName}”中不存在。");
                // 子对象类型
                type = property.PropertyType;
                propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
            }
        }
        else
        {
            propertyAccess = Expression.Property(parameter, field);
        }
        Conditions = Expression.Lambda<Func<TEntity, object>>(propertyAccess, parameter);
        SortDirection = sortDirection;
    }
    
    /// <summary>
    ///     排序字段表达式
    /// </summary>
    public Expression<Func<TEntity, object>> Conditions { set; get; }

    /// <summary>
    ///     排序方向
    /// </summary>
    public ListSortDirection SortDirection { set; get; } = ListSortDirection.Descending;
}