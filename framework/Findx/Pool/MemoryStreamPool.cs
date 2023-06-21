using Microsoft.IO;

namespace Findx;

/// <summary>
/// Stream池
/// </summary>
internal class MemoryStreamPool: IPool<MemoryStream>
{
    private readonly RecyclableMemoryStreamManager _memoryStreamManager = new();
    
    /// <summary>
    /// 获取
    /// </summary>
    /// <returns></returns>
    public MemoryStream Rent()
    {
        return _memoryStreamManager.GetStream();
    }

    /// <summary>
    /// 归还
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="clear"></param>
    public void Return(MemoryStream obj, bool clear = false)
    {
        obj?.Dispose();
    }
}