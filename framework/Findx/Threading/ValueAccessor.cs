namespace Findx.Threading;

/// <summary>
///     线程范型对象访问器
/// </summary>
/// <remarks>
///     AsyncLocal 为实例字段：每个 ValueAccessor 实例拥有独立的值槽，值沿异步调用链流动；
///     若改为 static 会导致同类型 T 的所有实例共享同一槽、互相串值。
/// </remarks>
public class ValueAccessor<T> : IValueAccessor<T>
{
    private readonly AsyncLocal<ValueHolder<T>> _asyncLocal = new();

    /// <summary>
    ///     获取或设置 值
    /// </summary>
    public T Value
    {
        get => _asyncLocal.Value != null ? _asyncLocal.Value.Value : default;
        set
        {
            _asyncLocal.Value ??= new ValueHolder<T>();
            _asyncLocal.Value.Value = value;
        }
    }
}