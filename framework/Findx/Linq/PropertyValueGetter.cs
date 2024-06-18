using Findx.Common;
using Findx.Extensions;
using Findx.Utilities;

namespace Findx.Linq;

/// <summary>
///     属性值读取器
/// </summary>
public static class PropertyValueGetter<TEntity> where TEntity : class
{
    private static readonly ConcurrentDictionary<string, Func<TEntity, object>> ReadPropertyValueDictionary = new ();
    
    /// <summary>
    ///     获取指定名称的公共属性的值
    /// </summary>
    /// <param name="entity">实例对象</param>
    /// <param name="propertyName">属性名称</param>
    /// <returns>值</returns>
    public static TReturn GetPropertyValue<TReturn>(TEntity entity, string propertyName)
    {
        Check.NotNull(entity, nameof(entity));
        Check.NotNullOrWhiteSpace(propertyName, nameof(propertyName));
        
        var key = $"{typeof(TEntity).FullName}.{propertyName}";
        
        var emitGetter = ReadPropertyValueDictionary.GetOrAdd(key, _ => PropertyUtility.EmitGetter<TEntity>(propertyName));
        
        return emitGetter(entity).CastTo<TReturn>();
    }
}