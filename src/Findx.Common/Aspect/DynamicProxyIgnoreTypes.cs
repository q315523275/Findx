using Findx.Extensions;

namespace Findx.Aspect;

/// <summary>
///     动态代理类型忽略
/// </summary>
public class DynamicProxyIgnoreTypes
{
    private static HashSet<Type> IgnoredTypes { get; } = new();

    /// <summary>
    ///     添加忽略类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void Add<T>()
    {
        lock (IgnoredTypes)
        {
            IgnoredTypes.TryAdd(typeof(T));
        }
    }

    /// <summary>
    ///     判断类型是否存在
    /// </summary>
    /// <param name="type"></param>
    /// <param name="includeDerivedTypes"></param>
    /// <returns></returns>
    public static bool Contains(Type type, bool includeDerivedTypes = true)
    {
        lock (IgnoredTypes)
        {
            return includeDerivedTypes
                ? IgnoredTypes.Any(t => t.IsAssignableFrom(type))
                : IgnoredTypes.Contains(type);
        }
    }
}