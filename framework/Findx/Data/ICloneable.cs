namespace Findx.Data;

/// <summary>
///     泛型浅克隆
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IClone<out T>
{
    /// <summary>
    ///     Clones the object.
    ///     <remarks>注意引用对象的变更影响</remarks>
    /// </summary>
    /// <returns>The cloned instance</returns>
    T Clone();
}