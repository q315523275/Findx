using System.Threading.Tasks;
using Findx.Common;
using FileSystemInfo = Findx.Common.FileSystemInfo;

namespace Findx.Storage;

/// <summary>
///     文件存储扩展
/// </summary>
public static class FileStorageExtensions
{
    /// <summary>
    ///     保存对象数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="storage"></param>
    /// <param name="path"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<bool> SaveObjectAsync<T>(this IFileStorage storage, string path, T data, CancellationToken cancellationToken = default)
    {
        Check.NotNull(path, nameof(path));

        var bytes = storage.Serializer.Serialize(data);
        return storage.SaveFileAsync(path, bytes, cancellationToken);
    }

    /// <summary>
    ///     获取对象数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="storage"></param>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<T> GetObjectAsync<T>(this IFileStorage storage, string path, CancellationToken cancellationToken = default)
    {
        Check.NotNull(path, nameof(path));

        var stream = await storage.GetFileStreamAsync(path, cancellationToken).ConfigureAwait(false);
        if (stream == null) return default;

        await using var _ = stream.ConfigureAwait(false);

        //  CopyToAsync 循环读满，避免单次 ReadAsync 短读导致截断；池化内存流降低大对象分配压力
        await using var ms = Pool.MemoryStream.Rent();
        await stream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);

        //  ToArray 返回独立拷贝，归还池后调用方数据仍然有效
        return storage.Serializer.Deserialize<T>(ms.ToArray());
    }

    /// <summary>
    ///     删除多个文件
    /// </summary>
    /// <param name="storage"></param>
    /// <param name="files"></param>
    /// <returns></returns>
    public static async Task DeleteFilesAsync(this IFileStorage storage, IEnumerable<FileSystemInfo> files)
    {
        Check.NotNull(files, nameof(files));

        foreach (var file in files) await storage.DeleteFileAsync(file.Path);
    }

    /// <summary>
    ///     获取文件字符串内容
    /// </summary>
    /// <param name="storage"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async Task<string> GetFileContentsAsync(this IFileStorage storage, string path)
    {
        Check.NotNull(path, nameof(path));

        await using var stream = await storage.GetFileStreamAsync(path);
        if (stream != null) return await new StreamReader(stream).ReadToEndAsync();

        return null;
    }

    /// <summary>
    ///     获取文件字节流
    /// </summary>
    /// <param name="storage"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async Task<byte[]> GetFileContentsRawAsync(this IFileStorage storage, string path)
    {
        Check.NotNull(path, nameof(path));

        await using var stream = await storage.GetFileStreamAsync(path);
        if (stream == null)
            return null;

        var buffer = new byte[16 * 1024];
        await using var ms = Pool.MemoryStream.Rent();
        int read;
        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0) 
            await ms.WriteAsync(buffer, 0, read);

        //  ToArray 返回独立拷贝，归还池后调用方数据仍然有效
        return ms.ToArray();
    }

    /// <summary>
    ///     保存字符串文件内容
    /// </summary>
    /// <param name="storage"></param>
    /// <param name="path"></param>
    /// <param name="contents"></param>
    /// <returns></returns>
    public static Task<bool> SaveFileAsync(this IFileStorage storage, string path, string contents)
    {
        Check.NotNull(path, nameof(path));

        //  直接走 byte[] 重载：存储实现不负责处置传入流（调用方拥有），绕开 MemoryStream 包装避免所有权问题
        return storage.SaveFileAsync(path, Encoding.UTF8.GetBytes(contents ?? string.Empty));
    }
}