using System.IO.Compression;
using System.Threading.Tasks;
using Findx.Extensions;

namespace Findx.Utilities;

/// <summary>
///     压缩工具类
/// </summary>
public static class CompressionUtility
{
    /// <summary>
    ///     对byte数组进行压缩(GZip)
    /// </summary>
    /// <param name="bytes">待压缩的byte数组</param>
    /// <returns>压缩后的byte数组</returns>
    public static async Task<byte[]> CompressAsync(byte[] bytes)
    {
        if (bytes is not { Length: > 0 }) return bytes;
        
        using var compressedStream = new MemoryStream();
        await using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
        {
            await zipStream.WriteAsync(bytes);
        }
        return compressedStream.ToArray();
    }

    /// <summary>
    ///     对byte[]数组进行解压(GZip)
    /// </summary>
    /// <param name="bytes">待解压的byte数组</param>
    /// <returns>解压后的byte数组</returns>
    public static async Task<byte[]> DecompressAsync(byte[] bytes)
    {
        if (bytes is not { Length: > 0 }) return bytes;

        using var originalStream = new MemoryStream(bytes);
        using var decompressedStream = new MemoryStream();
        await using (var decompressionStream = new GZipStream(originalStream, CompressionMode.Decompress))
        {
            await decompressionStream.CopyToAsync(decompressedStream);
        }
        return decompressedStream.ToArray();
    }

    /// <summary>
    ///     对字符串进行压缩(GZip)
    /// </summary>
    /// <param name="value">待压缩的字符串</param>
    /// <returns>压缩后的字符串(Base64String)</returns>
    public static async Task<string> CompressAsync(string value)
    {
        if (value.IsNullOrEmpty()) return string.Empty;
        var bytes = Encoding.UTF8.GetBytes(value);
        bytes = await CompressAsync(bytes);
        return Convert.ToBase64String(bytes.AsSpan());
    }

    /// <summary>
    ///     对字符串进行解压(GZip)
    /// </summary>
    /// <param name="value">待解压的字符串(Base64String)</param>
    /// <returns>解压后的字符串</returns>
    public static async Task<string> DecompressAsync(string value)
    {
        if (value.IsNullOrEmpty()) return string.Empty;
        var bytes = Convert.FromBase64String(value);
        bytes = await DecompressAsync(bytes);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    ///     字节数组压缩(Brotli)
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static async Task<byte[]> CompressByBrotliAsync(byte[] bytes)
    {
        if (bytes is not { Length: > 0 }) return bytes;
        
        using var compressedStream = new MemoryStream();
        await using (var zipStream = new BrotliStream(compressedStream, CompressionMode.Compress))
        {
            await zipStream.WriteAsync(bytes);
        }
        return compressedStream.ToArray();
    }

    /// <summary>
    ///     字节数组解压(Brotli)
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static async Task<byte[]> DecompressByBrotliAsync(byte[] bytes)
    {
        if (bytes is not { Length: > 0 }) return bytes;

        using var originalStream = new MemoryStream(bytes);
        using var decompressedStream = new MemoryStream();
        await using (var decompressionStream = new BrotliStream(originalStream, CompressionMode.Decompress))
        {
            await decompressionStream.CopyToAsync(decompressedStream);
        }
        return decompressedStream.ToArray();
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