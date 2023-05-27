using System.Threading.Tasks;

namespace Findx.Extensions;

/// <summary>
///     系统扩展 - 数据流
/// </summary>
public static partial class Extensions
{
    /// <summary>
    ///     Stream转换为byte[]
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static byte[] GetAllBytes(this Stream stream)
    {
        using (var memoryStream = new MemoryStream())
        {
            stream.Position = 0;
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }

    /// <summary>
    ///     Stream转换为byte[]
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<byte[]> GetAllBytesAsync(this Stream stream, CancellationToken cancellationToken = default)
    {
        using (var memoryStream = new MemoryStream())
        {
            stream.Position = 0;
            await stream.CopyToAsync(memoryStream, cancellationToken);
            return memoryStream.ToArray();
        }
    }

    /// <summary>
    ///     Stream拷贝
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="destination">目标Stream</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task CopyToAsync(this Stream stream, Stream destination, CancellationToken cancellationToken)
    {
        stream.Position = 0;
        return stream.CopyToAsync(destination, 81920, cancellationToken);
    }
}