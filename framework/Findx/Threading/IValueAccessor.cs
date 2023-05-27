namespace Findx.Threading;

/// <summary>
///     线程范型对象访问器
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IValueAccessor<T>
{
    /// <summary>
    ///     获取或设置 值
    /// </summary>
    T Value { set; get; }
}