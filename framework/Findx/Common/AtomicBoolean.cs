namespace Findx.Common;

/// <summary>
///     原子布尔对象
/// </summary>
public class AtomicBoolean
{
    private volatile int _value;

    /// <summary>
    ///     Ctor
    /// </summary>
    public AtomicBoolean() : this(false)
    {
    }

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="value"></param>
    public AtomicBoolean(bool value)
    {
        Value = value;
    }

    /// <summary>
    ///     对象结果值
    /// </summary>
    public bool Value
    {
        get => _value != 0;
        private init => _value = value ? 1 : 0;
    }

    /// <summary>
    ///     比较入参与原有值，相同则进行替换
    /// </summary>
    /// <param name="expected">比较值</param>
    /// <param name="update">替换更新值</param>
    /// <returns>替换更新结果</returns>
    public bool CompareAndSet(bool expected, bool update)
    {
        var expectedInt = expected ? 1 : 0;
        var updateInt = update ? 1 : 0;
        return Interlocked.CompareExchange(ref _value, updateInt, expectedInt) == expectedInt;
    }
}