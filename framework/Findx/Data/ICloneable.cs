namespace Findx.Data;

/// <summary>
///    基于 <see cref="ICloneable" /> 泛型浅隆
/// </summary>
public interface ICloneable<out T> : ICloneable
{
    /// <summary>
    /// Clones the object.
    /// </summary>
    /// <returns>The cloned instance</returns>
    new T Clone();
}