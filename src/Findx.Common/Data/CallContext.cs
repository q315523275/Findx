namespace Findx.Data;

/// <summary>
/// 线程内唯一对象
/// </summary>
public static class CallContext<T>
{
    private static readonly ConcurrentDictionary<string, AsyncLocal<T>> state = new();
    
    /// <summary>
    /// 设置对象
    /// </summary>
    /// <param name="name"></param>
    /// <param name="data"></param>
    public static void SetData(string name, T data) => state.GetOrAdd(name, _ => new AsyncLocal<T>()).Value = data;

    /// <summary>
    /// 获取对象
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T GetData(string name) => state.TryGetValue(name, out var data) ? data.Value : default(T);
}