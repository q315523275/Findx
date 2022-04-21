using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Findx.Serialization;
using Findx.Utils.Files;

namespace Findx.Storage
{
    /// <summary>
    /// 文件存储
    /// </summary>
    public interface IFileStorage : IDisposable
    {
        ISerializer Serializer { get; }

        string Name { get; }

        Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default);

        Task<FileSpec> GetFileInfoAsync(string path);

        Task<bool> ExistsAsync(string path);

        Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default);

        Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default);

        Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default);

        Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default);

        Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellation = default);
    }
}

