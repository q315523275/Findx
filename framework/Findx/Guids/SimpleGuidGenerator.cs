namespace Findx.Guids;

/// <summary>
///     原始Guid生成
/// </summary>
public class SimpleGuidGenerator : IGuidGenerator
{
    /// <summary>
    ///     实例
    /// </summary>
    public static SimpleGuidGenerator Instance { get; } = new();

    /// <summary>
    ///     生成
    /// </summary>
    /// <returns></returns>
    public virtual Guid Create()
    {
        return Guid.NewGuid();
    }
}