namespace Findx.Common;

/// <summary>
///     原子整型数据对象
/// </summary>
public class AtomicIntegerArray : AtomicReferenceArray<int>
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="length"></param>
    public AtomicIntegerArray(int length)
        : base(length)
    {
    }
}