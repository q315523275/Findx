namespace Findx.Utils;

/// <summary>
///     原子长整型对象
/// </summary>
public class AtomicLong
{
    private long _value;

    /// <summary>
    ///     Ctor
    /// </summary>
    public AtomicLong()
        : this(0)
    {
    }

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="value"></param>
    public AtomicLong(long value)
    {
        _value = value;
    }

    /// <summary>
    ///     对象结果值
    /// </summary>
    public long Value
    {
        get => Interlocked.Read(ref _value);

        set => Interlocked.Exchange(ref _value, value);
    }

    /// <summary>
    ///     比较入参与原有值，相同则进行替换
    /// </summary>
    /// <param name="expected">比较值</param>
    /// <param name="update">替换更新值</param>
    /// <returns>替换更新结果</returns>
    public bool CompareAndSet(long expected, long update)
    {
        return Interlocked.CompareExchange(ref _value, update, expected) == expected;
    }

    /// <summary>
    ///     获取原有值并设置新值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public long GetAndSet(long value)
    {
        return Interlocked.Exchange(ref _value, value);
    }

    /// <summary>
    ///     添加指定值并返回求和结果
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public long AddAndGet(long value)
    {
        return Interlocked.Add(ref _value, value);
    }
}