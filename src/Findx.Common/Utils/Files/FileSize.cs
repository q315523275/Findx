using Findx.Extensions;

namespace Findx.Utils.Files;

/// <summary>
///     文件大小
/// </summary>
public struct FileSize
{
    /// <summary>
    ///     初始化文件大小
    /// </summary>
    /// <param name="size">文件大小</param>
    /// <param name="unit">文件大小单位</param>
    public FileSize(long size, FileSizeUnit unit = FileSizeUnit.Byte)
    {
        Size = GetSize(size, unit);
    }

    /// <summary>
    ///     获取文件大小
    /// </summary>
    private static long GetSize(long size, FileSizeUnit unit)
    {
        switch (unit)
        {
            case FileSizeUnit.K:
                return size * 1024;
            case FileSizeUnit.M:
                return size * 1024 * 1024;
            case FileSizeUnit.G:
                return size * 1024 * 1024 * 1024;
            default:
                return size;
        }
    }

    /// <summary>
    ///     文件字节长度
    /// </summary>
    public long Size { get; }

    /// <summary>
    ///     获取文件大小，单位：字节
    /// </summary>
    public int GetSize()
    {
        return (int)Size;
    }

    /// <summary>
    ///     获取文件大小，单位：K
    /// </summary>
    public double GetSizeByK()
    {
        return (Size / 1024.0).To<double>();
    }

    /// <summary>
    ///     获取文件大小，单位：M
    /// </summary>
    public double GetSizeByM()
    {
        return (Size / 1024.0 / 1024.0).To<double>();
    }

    /// <summary>
    ///     获取文件大小，单位：G
    /// </summary>
    public double GetSizeByG()
    {
        return (Size / 1024.0 / 1024.0 / 1024.0).To<double>();
    }

    /// <summary>
    ///     输出描述
    /// </summary>
    public override string ToString()
    {
        if (Size >= 1024 * 1024 * 1024)
            return $"{GetSizeByG():0.00} {FileSizeUnit.G.ToDescription()}";
        if (Size >= 1024 * 1024)
            return $"{GetSizeByM():0.00} {FileSizeUnit.M.ToDescription()}";
        if (Size >= 1024)
            return $"{GetSizeByK():0.00} {FileSizeUnit.K.ToDescription()}";
        return $"{Size} {FileSizeUnit.Byte.ToDescription()}";
    }
}