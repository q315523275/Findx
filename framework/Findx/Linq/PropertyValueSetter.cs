using Findx.Common;
using Findx.Utilities;

namespace Findx.Linq;

/// <summary>
///     属性值设置器
/// </summary>
public static class PropertyValueSetter<TEntity> where TEntity : class
{
    private static readonly ConcurrentDictionary<string, Action<TEntity, object>> WritePropertyValueDictionary = new ();

    /// <summary>
    ///     设置指定名称的公共属性的值
    /// </summary>
    /// <param name="entity">实例对象</param>
    /// <param name="propertyName">属性名称</param>
    /// <param name="propertyValue">属性值</param>
    /// <returns>值</returns>
    public static void SetPropertyValue<TValue>(TEntity entity, string propertyName, TValue propertyValue)
    {
        Check.NotNull(entity, nameof(entity));
        Check.NotNullOrWhiteSpace(propertyName, nameof(propertyName));
        
        var key = $"{typeof(TEntity).FullName}.{propertyName}";
        var emitSetter = WritePropertyValueDictionary.GetOrAdd(key, _ => PropertyUtility.EmitSetter<TEntity>(propertyName));
        emitSetter(entity, propertyValue);
    }
}