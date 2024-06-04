namespace Findx.Data;

/// <summary>
///     提供特定类型的单例列表
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonList<T> : Singleton<ConcurrentBag<T>>
{
    static SingletonList()
    {
        Singleton<ConcurrentBag<T>>.Instance = [];
    }

    /// <summary>
    ///     获取指定类型的列表集合的单例实例
    /// </summary>
    public new static ConcurrentBag<T> Instance => Singleton<ConcurrentBag<T>>.Instance;
}