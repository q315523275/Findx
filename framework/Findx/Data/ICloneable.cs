namespace Findx.Data;

/// <summary>
///     泛型浅克隆
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICloneable<out T>
{
    /// <summary>
    ///     Clones the object.
    /// </summary>
    /// <returns>The cloned instance</returns>
    T Clone();
}