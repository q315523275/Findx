using System.Threading.Tasks;
using Findx.Common;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Serialization;
using Findx.Utilities;
using FileSystemInfo = Findx.Common.FileSystemInfo;

namespace Findx.Storage;

/// <summary>
///     文件系统存储器
/// </summary>
public class FileSystemStorage : IFileStorage, IServiceNameAware
{
    //  System.Threading.Lock 为 .NET 9+ 类型，net6.0/net8.0 目标框架不可用，保持 object 以三框架兼容
    private readonly object _lockObject = new();

    private readonly ILogger<FileSystemStorage> _logger;

    private readonly string _mediaRootFolder;

    private readonly bool _allowOutsideRoot;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="app"></param>
    /// <param name="logger"></param>
    /// <param name="settingProvider"></param>
    public FileSystemStorage(IObjectSerializer serializer, IApplicationContext app, ILogger<FileSystemStorage> logger, IConfiguration settingProvider)
    {
        Serializer = serializer;
        _logger = logger;

        _mediaRootFolder = settingProvider.GetValue<string>("Findx:Storage:Folder:DefaultFolder") ?? app.MapPath("~/");
        //  默认严格模式：所有操作限定在存储根目录内；仅挂载盘等特殊部署显式配置 Findx:Storage:Folder:AllowOutsideRoot=true 才放开
        _allowOutsideRoot = settingProvider.GetValue<bool>("Findx:Storage:Folder:AllowOutsideRoot");
    }

    /// <summary>
    ///     序列化工具
    /// </summary>
    public IObjectSerializer Serializer { get; }

    /// <summary>
    ///     文件存储类型名称
    /// </summary>
    public string Name => nameof(FileStorageType.Folder);

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
                var targetFullPath = ResolvePath(targetPath);
                var directory = Path.GetDirectoryName(targetFullPath);
                if (directory != null)
                    DirectoryUtility.CreateIfNotExists(directory);

                File.Copy(ResolvePath(path), targetFullPath, overwrite);
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
            File.Delete(ResolvePath(path));
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
        //  显式防御：null/空模式不再静默清空整个存储根目录
        if (string.IsNullOrWhiteSpace(searchPattern))
            throw new ArgumentNullException(nameof(searchPattern));

        var count = 0;

        //  仅显式传 "*" 时才允许清空整个存储根目录
        if (searchPattern == "*")
        {
            if (Directory.Exists(_mediaRootFolder))
            {
                count += Directory.EnumerateFiles(_mediaRootFolder, "*", SearchOption.AllDirectories).Count();
                Directory.Delete(_mediaRootFolder, true);
            }

            return Task.FromResult(count);
        }

        //  搜索模式同样走路径解析：默认严格模式拒绝越界/绝对路径，显式开启 AllowOutsideRoot 时保持原宽松行为
        searchPattern = searchPattern.NormalizePath();
        var path = ResolvePath(searchPattern);

        if (path[^1] == Path.DirectorySeparatorChar || path.EndsWith(Path.DirectorySeparatorChar + "*"))
        {
            var directory = Path.GetDirectoryName(path);
            if (Directory.Exists(directory))
            {
                count += Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Count();
                Directory.Delete(directory, true);
                return Task.FromResult(count);
            }

            return Task.FromResult(0);
        }

        if (Directory.Exists(path))
        {
            count += Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Count();
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
        return Task.FromResult(File.Exists(ResolvePath(path)));
    }

    /// <summary>
    ///     获取文件信息
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<FileSystemInfo> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
    {
        Check.NotNull(path, nameof(path));

        path = path.NormalizePath();

        var fileInfo = new FileInfo(ResolvePath(path));
        if (!fileInfo.Exists)
            return Task.FromResult<FileSystemInfo>(null);

        var fileSpec = new FileSystemInfo(path, fileInfo.Length, fileInfo.Name, Guid.NewGuid().ToString()) { SaveName = fileInfo.Name };

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
            return Task.FromResult<Stream>(File.OpenRead(ResolvePath(path)));
        }
        catch (IOException ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
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
                var newFullPath = ResolvePath(newPath);
                var directory = Path.GetDirectoryName(newFullPath);
                if (directory != null)
                    DirectoryUtility.CreateIfNotExists(directory);

                var oldFullPath = ResolvePath(path);
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
        var file = ResolvePath(path);

        try
        {
            //  保存整个流内容：从流起始位置拷贝
            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);
            
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
        var file = ResolvePath(path);

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
            DirectoryUtility.CreateIfNotExists(directory);

        return File.Create(filePath);
    }

    /// <summary>
    ///     解析存储路径
    ///     默认严格模式：规范化后必须落在存储根目录（_mediaRootFolder）内，绝对路径与 ../ 越界一律拒绝（防路径穿越）；
    ///     仅当显式配置 Findx:Storage:Folder:AllowOutsideRoot=true（如挂载盘场景）时才允许根目录之外的路径。
    /// </summary>
    /// <param name="path">存储路径（相对存储根目录）</param>
    /// <returns>规范化后的绝对路径</returns>
    private string ResolvePath(string path)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_mediaRootFolder, path));

        if (_allowOutsideRoot)
            return fullPath;

        //  前缀 containment 校验：root 统一补尾分隔符，避免 "/data/root2" 被 "/data/root" 前缀误判为合法
        var rootFullPath = Path.GetFullPath(_mediaRootFolder);
        var rootWithSeparator = rootFullPath.EndsWith(Path.DirectorySeparatorChar) ? rootFullPath : rootFullPath + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.Ordinal))
            throw new InvalidOperationException($"存储路径越界，仅允许在存储根目录内（{rootFullPath}）：{path}");

        return fullPath;
    }
}