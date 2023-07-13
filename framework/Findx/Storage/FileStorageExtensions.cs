using System.Threading.Tasks;
using Findx.Utils.Files;

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
    public static Task<bool> SaveObjectAsync<T>(this IFileStorage storage, string path, T data,
        CancellationToken cancellationToken = default)
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
    public static async Task<T> GetObjectAsync<T>(this IFileStorage storage, string path,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(path, nameof(path));

        var stream = await storage.GetFileStreamAsync(path, cancellationToken).ConfigureAwait(false);
        await using var _ = stream.ConfigureAwait(false);
        if (stream == null) return default;

        var bytes = new byte[stream.Length];
        // ReSharper disable once MustUseReturnValue
        await stream.ReadAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
        stream.Seek(0, SeekOrigin.Begin);

        return storage.Serializer.Deserialize<T>(bytes);
    }

    /// <summary>
    ///     删除多个文件
    /// </summary>
    /// <param name="storage"></param>
    /// <param name="files"></param>
    /// <returns></returns>
    public static async Task DeleteFilesAsync(this IFileStorage storage, IEnumerable<FileSpec> files)
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
        using var ms = new MemoryStream();
        int read;
        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0) 
            await ms.WriteAsync(buffer, 0, read);

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

        return storage.SaveFileAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(contents ?? string.Empty)));
    }
}