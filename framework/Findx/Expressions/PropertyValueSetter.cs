using Findx.Common;
using Findx.Utilities;

namespace Findx.Expressions;

/// <summary>
///     属性值设置器(字典存储委托)
/// </summary>
public static class PropertyValueSetter<TEntity> where TEntity : class
{
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, Action<TEntity, object>>> WritePropertyValueDictionary = new ();

    /// <summary>
    ///     设置指定名称的公共属性的值
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="entity">实例对象</param>
    /// <param name="propertyName">属性名称</param>
    /// <param name="propertyValue">属性值</param>
    /// <returns>值</returns>
    public static void SetPropertyValue<TValue>(Type entityType, TEntity entity, string propertyName, TValue propertyValue)
    {
        Check.NotNull(entity, nameof(entity));
        Check.NotNullOrWhiteSpace(propertyName, nameof(propertyName));
        
        var expressionSetter = WritePropertyValueDictionary.GetOrAdd(entityType, _ => new ConcurrentDictionary<string, Action<TEntity, object>>())
            .GetOrAdd(propertyName, PropertyUtility.ExpressionSetter<TEntity>);
        expressionSetter(entity, propertyValue);
    }

    /// <summary>
    ///     设置指定名称的公共属性的值
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="entity">实例对象</param>
    /// <param name="propertyName">属性名称</param>
    /// <param name="propertyValue">属性值</param>
    /// <returns>值</returns>
    public static void SetPropertyValueObject(Type entityType, TEntity entity, string propertyName, object propertyValue)
    {
        Check.NotNull(entity, nameof(entity));
        Check.NotNullOrWhiteSpace(propertyName, nameof(propertyName));
        
        var expressionSetter = WritePropertyValueDictionary.GetOrAdd(entityType, _ => new ConcurrentDictionary<string, Action<TEntity, object>>())
            .GetOrAdd(propertyName, PropertyUtility.ExpressionSetter<TEntity>);
        expressionSetter(entity, propertyValue);
    }
}