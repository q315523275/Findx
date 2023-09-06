namespace Findx.Data;

/// <summary>
/// 泛型浅克隆
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICloneable<out T> : ICloneable
{
    /// <summary>
    /// Clones the object.
    /// </summary>
    /// <returns>The cloned instance</returns>
    new T Clone();
}