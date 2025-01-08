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