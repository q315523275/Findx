namespace Findx.Threading;

/// <summary>
///     线程范型对象访问器
/// </summary>
public class ValueAccessor<T> : IValueAccessor<T>
{
    private static readonly AsyncLocal<ValueHolder<T>> AsyncLocal = new();

    /// <summary>
    ///     获取或设置 值
    /// </summary>
    public T Value
    {
        get => AsyncLocal.Value != null ? AsyncLocal.Value.Value : default;
        set
        {
            AsyncLocal.Value ??= new ValueHolder<T>();
            AsyncLocal.Value.Value = value;
        }
    }
}