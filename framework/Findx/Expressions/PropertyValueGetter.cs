using Findx.Common;
using Findx.Extensions;
using Findx.Utilities;

namespace Findx.Expressions;

/// <summary>
///     属性值读取器(字典存储委托)
/// </summary>
public static class PropertyValueGetter<TEntity> where TEntity : class
{
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, Func<TEntity, object>>> ReadPropertyValueDictionary = new ();

    /// <summary>
    ///     获取指定名称的公共属性的值
    /// </summary>
    /// <param name="entityType">实体类型</param>
    /// <param name="entity">实例对象</param>
    /// <param name="propertyName">属性名称</param>
    /// <returns>值</returns>
    public static TReturn GetPropertyValue<TReturn>(Type entityType, TEntity entity, string propertyName)
    {
        Check.NotNull(entity, nameof(entity));
        Check.NotNullOrWhiteSpace(propertyName, nameof(propertyName));

        var expressionGetter = ReadPropertyValueDictionary.GetOrAdd(entityType ?? entity.GetType(), _ => new ConcurrentDictionary<string, Func<TEntity, object>>())
            .GetOrAdd(propertyName, PropertyUtility.ExpressionGetter<TEntity>);
        return expressionGetter(entity).CastTo<TReturn>();
    }

    /// <summary>
    ///     获取指定名称的公共属性的值
    /// </summary>
    /// <param name="entityType">实体类型</param>
    /// <param name="entity">实例对象</param>
    /// <param name="propertyName">属性名称</param>
    /// <returns>值</returns>
    public static object GetPropertyValueObject(Type entityType, TEntity entity, string propertyName)
    {
        Check.NotNull(entity, nameof(entity));
        Check.NotNullOrWhiteSpace(propertyName, nameof(propertyName));
        
        var expressionGetter = ReadPropertyValueDictionary.GetOrAdd(entityType ?? entity.GetType(), _ => new ConcurrentDictionary<string, Func<TEntity, object>>())
            .GetOrAdd(propertyName, PropertyUtility.ExpressionGetter<TEntity>);
        return expressionGetter(entity);
    }
}