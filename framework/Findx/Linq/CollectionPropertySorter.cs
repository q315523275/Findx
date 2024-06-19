using System.Linq.Expressions;

namespace Findx.Linq;

/// <summary>
///     集合属性排序(缓存)
/// </summary>
public static class CollectionPropertySorter<TEntity>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ConcurrentDictionary<string, LambdaExpression> PropertyAccessDict = new();
    
    /// <summary>
    ///     LambdaExpression
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static LambdaExpression GetKeySelector(string field)
    {
        var entityType = typeof(TEntity);
        var key = entityType.FullName + "." + field;
        if (PropertyAccessDict.TryGetValue(key, out var selector))
        {
            return selector;
        }
        
        var parameter = Expression.Parameter(entityType, "x");
        Expression propertyAccess = parameter;
        var propertyNames = field.Split('.');
        if (propertyNames.Length > 1)
        {
            var propertyType = parameter.Type;
            foreach (var propertyName in propertyNames)
            {
                var property = propertyType.GetProperty(propertyName);
                if (property == null)
                    throw new InvalidOperationException($"指定的属性“{propertyName}”在类型“{propertyType.FullName}”中不存在。");
                // 子对象类型
                propertyType = property.PropertyType;
                propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
            }
        }
        else
        {
            propertyAccess = Expression.Property(parameter, field);
        }
        
        // var keySelector = Expression.Lambda(propertyAccess, parameter);
        // 适配Orm，多一层转换
        var keySelector = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(propertyAccess, typeof(object)), parameter);
        
        PropertyAccessDict[key] = keySelector;
        
        return keySelector;
    }
}