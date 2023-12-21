using System.Diagnostics;
using System.Threading.Tasks;
using Findx.Common;

namespace Findx.Extensions;

/// <summary>
///     系统扩展 - 数据流
/// </summary>
public static partial class Extensions
{
    /// <summary>
    ///     stream 转换为 string
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static Task<string> AsStringAsync(this Stream stream) => AsStringInternalAsync(stream, Encoding.UTF8);
    
    /// <summary>
    ///     stream 转换为 string
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static Task<string> AsStringAsync(this Stream stream, Encoding encoding) => AsStringInternalAsync(stream, encoding);
    
    /// <summary>
    ///     stream 转换为 string 内部实现
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static async Task<string> AsStringInternalAsync(Stream stream, Encoding encoding)
    {
        if (encoding == null)
        {
            throw new ArgumentNullException(nameof(encoding));
        }

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using var sr = new StreamReader(stream, encoding);
        sr.BaseStream.Seek(0, SeekOrigin.Begin);
        return await sr.ReadToEndAsync();
    }
    
    /// <summary>
    ///     stream 转换为 byte 
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static Task<byte[]> ToByteArrayAsync(this Stream stream) => ToByteArrayInternalAsync(stream);
    
    /// <summary>
    ///     stream 转换为 byte 内部实现
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static async Task<byte[]> ToByteArrayInternalAsync(Stream stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (stream is MemoryStream mem)
        {
            return mem.TryGetBuffer(out var buffer) ? buffer.Array : mem.ToArray();
        }

        try
        {
            var len = stream.Length;
            if (len > int.MaxValue)
            {
                return await ToByteArrayCopyAsync(stream);
            }

            var buffer = new byte[(int)len];

            _ = await stream.ReadAsync(buffer.AsMemory(0, (int)len));

            return buffer;
        }
        catch
        {
            return await ToByteArrayCopyAsync(stream);
        }

        static async Task<byte[]> ToByteArrayCopyAsync(Stream stream)
        {
            using var memStream = new MemoryStream();
            await stream.CopyToAsync(memStream);
            return memStream.ToArray();
        }
    }
    
    /// <summary>
    ///     向 MemoryStream 写入字符串数据
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="value"></param>
    /// <param name="seekToBegin"></param>
    /// <returns></returns>
    public static MemoryStream WriteString(this MemoryStream stream, string value, bool seekToBegin = true)
    {
        Check.NotNull(stream, nameof(stream));
        Check.NotNull(value, nameof(value));

        using var writer = new StreamWriter(stream, Encoding.Unicode, 1024, true);

        writer.Write(value);
        writer.Flush();

        if (seekToBegin)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        return stream;
    }
    
    /// <summary>
    ///     拷贝数据流到文件中
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="destinationPath"></param>
    /// <param name="leaveOpen"></param>
    /// <returns></returns>
    public static async Task<bool> CopyToFileAsync(this Stream stream, string destinationPath, bool leaveOpen = true)
    {
        Check.NotNull(stream, nameof(stream));
        Check.NotNullOrWhiteSpace(destinationPath, nameof(destinationPath));

        try
        {
            await using (var outStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await stream.CopyToAsync(outStream);
            }

            return File.Exists(destinationPath);
        }
        catch (Exception ex)
        {
            Debug.Fail(ex.Message);
            return false;
        }
        finally
        {
            if (leaveOpen)
            {
                if (stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }
            }
            else
            {
                stream.Close();
                await stream.DisposeAsync();
            }
        }
    }
    
    /// <summary>
    ///     比较两个数据流内容是否相等
    /// </summary>
    /// <param name="src"></param>
    /// <param name="other"></param>
    /// <param name="forceLengthCompare"></param>
    /// <returns></returns>
    public static Task<bool> ContentsEqualAsync(this Stream src, Stream other, bool? forceLengthCompare = null) => ContentsEqualInternalAsync(src, other, forceLengthCompare);

    /// <summary>
    ///     比较两个数据流内容是否相等
    /// </summary>
    /// <param name="src"></param>
    /// <param name="other"></param>
    /// <param name="forceLengthCompare"></param>
    /// <returns></returns>
    private static async Task<bool> ContentsEqualInternalAsync(Stream src, Stream other, bool? forceLengthCompare)
    {
        Check.NotNull(src, nameof(src));
        Check.NotNull(other, nameof(other));

        if (src == other)
        {
            return true;
        }

        if ((!forceLengthCompare.HasValue && src.CanSeek && other.CanSeek) || (forceLengthCompare == true))
        {
            if (src.Length != other.Length)
            {
                return false;
            }
        }

        const int intSize = sizeof(long);
        const int bufferSize = 1024 * intSize; // 2048;
        var buffer1 = new byte[bufferSize];
        var buffer2 = new byte[bufferSize];

        while (true)
        {
            var len1 = await src.ReadAsync(buffer1.AsMemory(0, bufferSize));
            var len2 = await other.ReadAsync(buffer2.AsMemory(0, bufferSize));

            if (len1 != len2)
                return false;

            if (len1 == 0)
                return true;

            var iterations = (int)Math.Ceiling((double)len1 / sizeof(long));

            for (var i = 0; i < iterations; i++)
            {
                if (BitConverter.ToInt64(buffer1, i * intSize) != BitConverter.ToInt64(buffer2, i * intSize))
                {
                    return false;
                }
            }

            return true;
        }
    }
}