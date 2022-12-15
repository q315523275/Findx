namespace Findx.Data;

/// <summary>
/// 提供特定类型的单例列表
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonList<T> : Singleton<IList<T>>
{
    static SingletonList()
    {
        Singleton<IList<T>>.Instance = new List<T>();
    }

    /// <summary>
    /// 获取指定类型的列表集合的单例实例
    /// </summary>
    public static new IList<T> Instance => Singleton<IList<T>>.Instance;
}