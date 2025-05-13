namespace Findx.Threading;

/// <summary>
///     线程范型对象访问器
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>使用static AsyncLocal实现</remarks>
public interface IValueAccessor<T>
{
    /// <summary>
    ///     获取或设置 值
    /// </summary>
    T Value { set; get; }
}