namespace Findx;

/// <summary>
///     值Null检测
///     <para>隐式运算符</para>
/// </summary>
/// <typeparam name="T"></typeparam>
public class NotNull<T>
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="value"></param>
    public NotNull(T value)
    {
        Value = value;
    }

    /// <summary>
    ///     对象值
    /// </summary>
    public T Value { get; }

    /// <summary>
    ///     判断是否为Null
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">为Null抛出异常</exception>
    public static implicit operator NotNull<T>(T value)
    {
        if (value == null)
            throw new ArgumentNullException();

        return new NotNull<T>(value);
    }
}