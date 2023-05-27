namespace Findx.Extensions;

/// <summary>
///     系统扩展 - 字典
/// </summary>
public static partial class Extensions
{
    internal static bool TryGetValue<T>(this IDictionary<string, object> dictionary, string key, out T value)
    {
        if (dictionary.TryGetValue(key, out var valueObj) && valueObj is T t)
        {
            value = t;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    ///     获取字典值,不存在则返回默认值
    /// </summary>
    /// <typeparam name="TKey">字典Key泛型</typeparam>
    /// <typeparam name="TValue">字典Value泛型</typeparam>
    /// <param name="dictionary">字典对象</param>
    /// <param name="key">字典Key</param>
    /// <returns></returns>
    public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
    {
        return dictionary.TryGetValue(key, out var obj) ? obj : default;
    }

    /// <summary>
    ///     获取字典值,不存在则返回默认值
    /// </summary>
    /// <typeparam name="TKey">字典Key泛型</typeparam>
    /// <typeparam name="TValue">字典Value泛型</typeparam>
    /// <param name="dictionary">字典接口对象</param>
    /// <param name="key">字典Key</param>
    /// <returns></returns>
    public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        return dictionary.TryGetValue(key, out var obj) ? obj : default;
    }

    /// <summary>
    ///     获取字典值,不存在则返回默认值
    /// </summary>
    /// <typeparam name="TKey">字典Key泛型</typeparam>
    /// <typeparam name="TValue">字典Value泛型</typeparam>
    /// <param name="dictionary">只读字典对象</param>
    /// <param name="key">字典Key</param>
    /// <returns></returns>
    public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
    {
        return dictionary.TryGetValue(key, out var obj) ? obj : default;
    }

    /// <summary>
    ///     获取字典值,不存在则返回默认值
    /// </summary>
    /// <typeparam name="TKey">字典Key泛型</typeparam>
    /// <typeparam name="TValue">字典Value泛型</typeparam>
    /// <param name="dictionary">线程安装字典对象</param>
    /// <param name="key">字典Key</param>
    /// <returns></returns>
    public static TValue GetOrDefault<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
    {
        return dictionary.TryGetValue(key, out var obj) ? obj : default;
    }

    /// <summary>
    ///     获取字典值,不存在则插入新字典至
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
        Func<TKey, TValue> factory)
    {
        if (dictionary.TryGetValue(key, out var obj)) return obj;

        return dictionary[key] = factory(key);
    }

    /// <summary>
    ///     获取字典值,不存在则插入新字典至
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="factory">如果在字典中找不到值，则用于创建值的工厂方法</param>
    /// <returns></returns>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
        Func<TValue> factory)
    {
        return dictionary.GetOrAdd(key, k => factory());
    }
}