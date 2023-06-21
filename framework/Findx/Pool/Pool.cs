namespace Findx;

/// <summary>
/// 池
/// </summary>
public static class Pool
{
    /// <summary>
    /// 内存流
    /// </summary>
    public static IPool<MemoryStream> MemoryStream { get; } = new MemoryStreamPool();
}