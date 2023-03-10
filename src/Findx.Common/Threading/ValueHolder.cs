namespace Findx.Threading;

/// <summary>
/// 值承载器
/// </summary>
/// <typeparam name="T"></typeparam>
public class ValueHolder<T>
{
    /// <summary>
    /// 获取或设置 值
    /// </summary>
    public T Value { get; set; }
}