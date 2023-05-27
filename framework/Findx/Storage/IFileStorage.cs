using System.Threading.Tasks;
using Findx.Serialization;
using Findx.Utils.Files;

namespace Findx.Storage;

/// <summary>
///     文件存储
/// </summary>
public interface IFileStorage : IDisposable
{
    /// <summary>
    ///     序列化器
    /// </summary>
    ISerializer Serializer { get; }

    /// <summary>
    ///     存储提供器名称
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     获取文件数据流
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    ///     获取文件详情信息
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Task<FileSpec> GetFileInfoAsync(string path);

    /// <summary>
    ///     判断文件是否存在
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Task<bool> ExistsAsync(string path);

    /// <summary>
    ///     保存文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default);

    /// <summary>
    ///     保存文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="byteArray"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> SaveFileAsync(string path, byte[] byteArray, CancellationToken cancellationToken = default);

    /// <summary>
    ///     重命名文件名
    /// </summary>
    /// <param name="path"></param>
    /// <param name="newPath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default);

    /// <summary>
    ///     复制文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="targetPath"></param>
    /// <param name="overwrite">目标存在时是否覆盖</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> CopyFileAsync(string path, string targetPath, bool overwrite = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     删除文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    ///     批量删除文件
    /// </summary>
    /// <param name="searchPattern"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellation = default);
}