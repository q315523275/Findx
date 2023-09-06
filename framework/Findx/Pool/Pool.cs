using Microsoft.Extensions.ObjectPool;

namespace Findx;

/// <summary>
///     池
/// </summary>
public static class Pool
{
    /// <summary>
    ///     内存流
    /// </summary>
    public static IPool<MemoryStream> MemoryStream { get; } = new MemoryStreamPool();
    
    /// <summary>
    ///     字符串构建器
    /// </summary>
    public static ObjectPool<StringBuilder> StringBuilder { get; } = new DefaultObjectPoolProvider().CreateStringBuilderPool();
}