using System.IO.Compression;
using System.Threading.Tasks;
using Findx.Common;
using Findx.Extensions;

namespace Findx.Utilities;

/// <summary>
///     压缩工具类
/// </summary>
public class CompressionUtility
{
    /// <summary>
    ///     对byte数组进行压缩
    /// </summary>
    /// <param name="buffer">待压缩的byte数组</param>
    /// <returns>压缩后的byte数组</returns>
    public static async Task<byte[]> CompressAsync(byte[] buffer)
    {
        Check.NotNull(buffer, nameof(buffer));

        using var compressedStream = new MemoryStream();
        await using var zipStream = new GZipStream(compressedStream, CompressionMode.Compress);
        await zipStream.WriteAsync(buffer);
        zipStream.Close();
        return compressedStream.ToArray();
    }

    /// <summary>
    ///     对byte[]数组进行解压
    /// </summary>
    /// <param name="buffer">待解压的byte数组</param>
    /// <returns>解压后的byte数组</returns>
    public static async Task<byte[]> DecompressAsync(byte[] buffer)
    {
        Check.NotNull(buffer, nameof(buffer));

        using var compressedStream = new MemoryStream(buffer);
        await using var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        await zipStream.CopyToAsync(resultStream);
        return resultStream.ToArray();
    }

    /// <summary>
    ///     对字符串进行压缩
    /// </summary>
    /// <param name="value">待压缩的字符串</param>
    /// <returns>压缩后的字符串</returns>
    public static async Task<string> CompressAsync(string value)
    {
        if (value.IsNullOrEmpty()) return string.Empty;
        var bytes = Encoding.UTF8.GetBytes(value);
        bytes = await CompressAsync(bytes);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    ///     对字符串进行解压
    /// </summary>
    /// <param name="value">待解压的字符串</param>
    /// <returns>解压后的字符串</returns>
    public static async Task<string> DecompressAsync(string value)
    {
        if (value.IsNullOrEmpty()) return string.Empty;
        var bytes = Convert.FromBase64String(value);
        bytes = await DecompressAsync(bytes);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// 将文件夹压缩成zip文件
    /// </summary>
    /// <param name="sourceDir">源需要压缩文件夹</param>
    /// <param name="zipFile">压缩包文件路径</param>
    public static void Zip(string sourceDir, string zipFile)
    {
        ZipFile.CreateFromDirectory(sourceDir, zipFile);
    }

    /// <summary>
    /// 将zip文件解压到指定文件夹
    /// </summary>
    /// <param name="zipFile">压缩包文件路径</param>
    /// <param name="targetDir">解压目标文件夹</param>
    public static void UnZip(string zipFile, string targetDir)
    {
        ZipFile.ExtractToDirectory(zipFile, targetDir);
    }
}