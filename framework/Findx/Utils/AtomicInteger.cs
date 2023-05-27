namespace Findx.Utils;

/// <summary>
///     原子整型对象
/// </summary>
public class AtomicInteger
{
    /// <summary>
    ///     对象结果值
    /// </summary>
    private int _value;

    /// <summary>
    ///     Ctor
    /// </summary>
    public AtomicInteger()
        : this(0)
    {
    }

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="value"></param>
    public AtomicInteger(int value)
    {
        _value = value;
    }

    /// <summary>
    ///     对象结果值
    /// </summary>
    public int Value
    {
        get => _value;

        set => Interlocked.Exchange(ref _value, value);
    }

    /// <summary>
    ///     比较入参与原有值，相同则进行替换
    /// </summary>
    /// <param name="expected">比较值</param>
    /// <param name="update">替换更新值</param>
    /// <returns>替换更新结果</returns>
    public bool CompareAndSet(int expected, int update)
    {
        return Interlocked.CompareExchange(ref _value, update, expected) == expected;
    }

    /// <summary>
    ///     递增并获取
    /// </summary>
    /// <returns></returns>
    public int IncrementAndGet()
    {
        return Interlocked.Increment(ref _value);
    }

    /// <summary>
    ///     递减并获取
    /// </summary>
    /// <returns></returns>
    public int DecrementAndGet()
    {
        return Interlocked.Decrement(ref _value);
    }

    /// <summary>
    ///     获取原有值并递增
    /// </summary>
    /// <returns></returns>
    public int GetAndIncrement()
    {
        return Interlocked.Increment(ref _value) - 1;
    }

    /// <summary>
    ///     添加指定数值并返回添加后的结果
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public int AddAndGet(int value)
    {
        return Interlocked.Add(ref _value, value);
    }
}