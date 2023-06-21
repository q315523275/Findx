using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Serialization;
using Findx.Utils;
using Findx.Utils.Files;

namespace Findx.Storage;

/// <summary>
///     本地文件存储器
/// </summary>
public class FolderFileStorage : IFileStorage, IServiceNameAware
{
    private readonly object _lockObject = new();

    private readonly ILogger<FolderFileStorage> _logger;

    private readonly string _mediaRootFolder;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="app"></param>
    /// <param name="logger"></param>
    /// <param name="settingProvider"></param>
    public FolderFileStorage(ISerializer serializer, IApplicationContext app, ILogger<FolderFileStorage> logger, IConfiguration settingProvider)
    {
        Serializer = serializer;
        _logger = logger;

        _mediaRootFolder = settingProvider.GetValue<string>("Findx:Storage:Folder:DefaultFolder") ?? app.MapPath("~/");
    }

    /// <summary>
    ///     序列化工具
    /// </summary>
    public ISerializer Serializer { get; }

    /// <summary>
    ///     文件存储类型名称
    /// </summary>
    public string Name => FileStorageType.Folder.ToString();

    /// <summary>
    ///     文件拷贝
    /// </summary>
    /// <param name="path"></param>
    /// <param name="targetPath"></param>
    /// <param name="overwrite"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<bool> CopyFileAsync(string path, string targetPath, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        Check.NotNull(path, nameof(path));
        Check.NotNull(targetPath, nameof(targetPath));

        path = path.NormalizePath();

        try
        {
            lock (_lockObject)
            {
                var directory = Path.GetDirectoryName(targetPath);
                if (directory != null)
                    DirectoryTool.CreateIfNotExists(Path.Combine(_mediaRootFolder, directory));

                File.Copy(Path.Combine(_mediaRootFolder, path), Path.Combine(_mediaRootFolder, targetPath), overwrite);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error trying to copy file {Path} to {TargetPath}", path, targetPath);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    /// <summary>
    ///     删除文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        path = path.NormalizePath();

        try
        {
            File.Delete(Path.Combine(_mediaRootFolder, path));
        }
        catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
        {
            _logger.LogDebug(ex, "Error trying to delete file: {Path}", path);

            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    /// <summary>
    ///     删除匹配文件集合
    /// </summary>
    /// <param name="searchPattern"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellation = default)
    {
        var count = 0;

        if (searchPattern == null || string.IsNullOrEmpty(searchPattern) || searchPattern == "*")
        {
            if (Directory.Exists(_mediaRootFolder))
            {
                count += Directory.EnumerateFiles(_mediaRootFolder, "*,*", SearchOption.AllDirectories).Count();
                Directory.Delete(_mediaRootFolder, true);
            }

            return Task.FromResult(count);
        }

        searchPattern = searchPattern.NormalizePath();
        var path = Path.Combine(_mediaRootFolder, searchPattern);

        if (path[^1] == Path.DirectorySeparatorChar || path.EndsWith(Path.DirectorySeparatorChar + "*"))
        {
            var directory = Path.GetDirectoryName(path);
            if (Directory.Exists(directory))
            {
                count += Directory.EnumerateFiles(directory, "*,*", SearchOption.AllDirectories).Count();
                Directory.Delete(directory, true);
                return Task.FromResult(count);
            }

            return Task.FromResult(0);
        }

        if (Directory.Exists(path))
        {
            count += Directory.EnumerateFiles(path, "*,*", SearchOption.AllDirectories).Count();
            Directory.Delete(path, true);
            return Task.FromResult(count);
        }

        foreach (var file in Directory.EnumerateFiles(_mediaRootFolder, searchPattern, SearchOption.AllDirectories))
        {
            File.Delete(file);
            count++;
        }

        return Task.FromResult(count);
    }

    /// <summary>
    ///     文件是否存在
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        Check.NotNull(path, nameof(path));

        path = path.NormalizePath();
        return Task.FromResult(File.Exists(Path.Combine(_mediaRootFolder, path)));
    }

    /// <summary>
    ///     获取文件信息
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<FileSpec> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
    {
        Check.NotNull(path, nameof(path));

        path = path.NormalizePath();

        var fileInfo = new FileInfo(Path.Combine(_mediaRootFolder, path));
        if (!fileInfo.Exists)
            return Task.FromResult<FileSpec>(null);

        var fileSpec = new FileSpec(path, fileInfo.Length, fileInfo.Name, Guid.NewGuid().ToString())
        {
            SaveName = fileInfo.Name
        };

        return Task.FromResult(fileSpec);
    }

    /// <summary>
    ///     获取文件数据流
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
    {
        Check.NotNull(path, nameof(path));

        path = path.NormalizePath();

        try
        {
            return Task.FromResult<Stream>(File.OpenRead(Path.Combine(_mediaRootFolder, path)));
        }
        catch (IOException ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace(ex, "Error trying to get file stream: {Path}", path);

            return Task.FromResult<Stream>(null);
        }
    }

    /// <summary>
    ///     修改文件目录
    /// </summary>
    /// <param name="path"></param>
    /// <param name="newPath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
    {
        Check.NotNull(path, nameof(path));
        Check.NotNull(newPath, nameof(newPath));

        path = path.NormalizePath();
        newPath = newPath.NormalizePath();

        try
        {
            lock (_lockObject)
            {
                var directory = Path.GetDirectoryName(newPath);
                if (directory != null)
                    DirectoryTool.CreateIfNotExists(Path.Combine(_mediaRootFolder, directory));

                var oldFullPath = Path.Combine(_mediaRootFolder, path);
                var newFullPath = Path.Combine(_mediaRootFolder, newPath);
                try
                {
                    File.Move(oldFullPath, newFullPath);
                }
                catch (IOException)
                {
                    File.Delete(newFullPath);
                    File.Move(oldFullPath, newFullPath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error trying to rename file {Path} to {NewPath}", path, newPath);

            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    /// <summary>
    ///     保存文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default)
    {
        Check.NotNull(path, nameof(path));
        Check.NotNull(stream, nameof(stream));

        path = path.NormalizePath();
        var file = Path.Combine(_mediaRootFolder, path);

        try
        {
            await using var fileStream = CreateFileStream(file);
            await stream.CopyToAsync(fileStream, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error trying to save file: {Path}", path);
            return false;
        }
    }

    /// <summary>
    ///     保存文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="byteArray"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> SaveFileAsync(string path, byte[] byteArray, CancellationToken cancellationToken = default)
    {
        Check.NotNull(path, nameof(path));
        Check.NotNull(byteArray, nameof(byteArray));

        path = path.NormalizePath();
        var file = Path.Combine(_mediaRootFolder, path);

        try
        {
            await using var fileStream = CreateFileStream(file);
            await fileStream.WriteAsync(byteArray, 0, byteArray.Length, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error trying to save file: {Path}", path);
            return false;
        }
    }

    /// <summary>
    ///     创建真实文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private static Stream CreateFileStream(string filePath)
    {
        try
        {
            return File.Create(filePath);
        }
        catch (DirectoryNotFoundException)
        {
        }

        var directory = Path.GetDirectoryName(filePath);
        if (directory != null)
            DirectoryTool.CreateIfNotExists(directory);

        return File.Create(filePath);
    }
}