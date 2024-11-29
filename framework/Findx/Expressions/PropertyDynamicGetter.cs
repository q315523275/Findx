using System.Linq.Expressions;

namespace Findx.Expressions;

/// <summary>
///     属性值动态读取器(性能高于字典缓存)
/// </summary>
/// <typeparam name="T"></typeparam>
public class PropertyDynamicGetter<T>
{
    private static Func<T, string, object> _cachedGetDelegate;

    /// <summary>
    ///     Ctor
    /// </summary>
    public PropertyDynamicGetter()
    {
        if (_cachedGetDelegate == null)
        {
            var properties = typeof(T).GetProperties();
            _cachedGetDelegate = BuildDynamicGetDelegate(properties);
        }
    }

    /// <summary>
    ///     执行属性值动态返回
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public object GetPropertyValue(T obj, string propertyName)
    {
        return _cachedGetDelegate(obj, propertyName);
    }

    /// <summary>
    ///     构建属性读取表
    /// </summary>
    /// <param name="properties"></param>
    /// <returns></returns>
    private static Func<T, string, object> BuildDynamicGetDelegate(PropertyInfo[] properties)
    {
        var objParamExpression = Expression.Parameter(typeof(T), "obj");
        var nameParamExpression = Expression.Parameter(typeof(string), "name");
        var variableExpression = Expression.Variable(typeof(object), "propertyValue");

        var switchCases = new List<SwitchCase>();
        foreach (var property in properties)
        {
            var getPropertyExpression = Expression.Property(objParamExpression, property);
            var convertPropertyExpression = Expression.Convert(getPropertyExpression, typeof(object));
            var assignExpression = Expression.Assign(variableExpression, convertPropertyExpression);
            var switchCase = Expression.SwitchCase(assignExpression, Expression.Constant(property.Name));
            switchCases.Add(switchCase);
        }

        // set null when default
        var defaultBodyExpression = Expression.Assign(variableExpression, Expression.Constant(null));
        var switchExpression = Expression.Switch(nameParamExpression, defaultBodyExpression, switchCases.ToArray());
        var blockExpression = Expression.Block(typeof(object), [variableExpression], switchExpression);
        var lambdaExpression = Expression.Lambda<Func<T, string, object>>(blockExpression, objParamExpression, nameParamExpression);
        return lambdaExpression.Compile();
    }
}