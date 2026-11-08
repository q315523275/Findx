namespace Findx.Aspect;

/// <summary>
///     代理工具
/// </summary>
public static class ProxyHelper
{
    private const string ProxyNamespace = "Castle.Proxies";

    /// <summary>
    ///     按代理类型缓存 __target 字段信息，避免每次调用都反射枚举字段
    /// </summary>
    private static readonly ConcurrentDictionary<Type, FieldInfo> TargetFieldCache = new();

    /// <summary>
    ///     取消代理
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static object UnProxy(object obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        if (obj.GetType().Namespace != ProxyNamespace) return obj;

        var targetField = TargetFieldCache.GetOrAdd(obj.GetType(),
            type => type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(f => f.Name == "__target"));

        if (targetField == null) return obj;

        return targetField.GetValue(obj);
    }

    /// <summary>
    ///     获取未代理类型
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Type GetUnProxiedType(object obj)
    {
        return UnProxy(obj).GetType();
    }
}