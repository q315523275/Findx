namespace Findx.Extensions;

/// <summary>
///     系统扩展 - 比较
/// </summary>
public static partial class Extensions
{
    /// <summary>
    ///     泛型比较
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="minInclusiveValue"></param>
    /// <param name="maxInclusiveValue"></param>
    /// <returns></returns>
    public static bool IsBetween<T>(this T value, T minInclusiveValue, T maxInclusiveValue) where T : IComparable<T>
    {
        return value.CompareTo(minInclusiveValue) >= 0 && value.CompareTo(maxInclusiveValue) <= 0;
    }
}