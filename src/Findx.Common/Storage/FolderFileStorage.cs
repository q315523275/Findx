using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Findx.Extensions;
using Findx.Serialization;
using Findx.Setting;
using Findx.Utils.Files;
using Microsoft.Extensions.Logging;

namespace Findx.Storage
{
    public class FolderFileStorage : IFileStorage
    {

        /// <summary>
        /// 序列化工具
        /// </summary>
        public ISerializer Serializer { get; private set; }

        /// <summary>
        /// 文件存储类型名称
        /// </summary>
        public string Name => FileStorageType.Folder.ToString();

        private readonly ILogger<FolderFileStorage> _logger;

        private readonly string MediaRootFoler = "store";

        private readonly object _lockObject = new();

        public FolderFileStorage(ISerializer serializer, ILogger<FolderFileStorage> logger, ISettingProvider setting)
        {
            Serializer = serializer;
            _logger = logger;

            MediaRootFoler = setting.GetValue<string>("Storage:Folder:DefaultFolder") ?? "store";
        }

        public void Dispose() { }

        /// <summary>
        /// 文件拷贝
        /// </summary>
        /// <param name="path"></param>
        /// <param name="targetPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default)
        {
            Check.NotNull(path, nameof(path));
            Check.NotNull(targetPath, nameof(targetPath));

            path = path.NormalizePath();

            try
            {
                lock (_lockObject)
                {
                    string directory = Path.GetDirectoryName(targetPath);
                    if (directory != null)
                        Directory.CreateDirectory(Path.Combine(MediaRootFoler, directory));

                    File.Copy(Path.Combine(MediaRootFoler, path), Path.Combine(MediaRootFoler, targetPath));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error trying to copy file {Path} to {TargetPath}.", path, targetPath);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            path = path.NormalizePath();

            try
            {
                File.Delete(Path.Combine(MediaRootFoler, path));
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                _logger.LogDebug(ex, "Error trying to delete file: {Path}.", path);

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// 删除匹配文件集合
        /// </summary>
        /// <param name="searchPattern"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellation = default)
        {
            int count = 0;

            if (searchPattern == null || String.IsNullOrEmpty(searchPattern) || searchPattern == "*")
            {
                if (Directory.Exists(MediaRootFoler))
                {
                    count += Directory.EnumerateFiles(MediaRootFoler, "*,*", SearchOption.AllDirectories).Count();
                    Directory.Delete(MediaRootFoler, true);
                }

                return Task.FromResult(count);
            }

            searchPattern = searchPattern.NormalizePath();
            string path = Path.Combine(MediaRootFoler, searchPattern);

            if (path[path.Length - 1] == Path.DirectorySeparatorChar || path.EndsWith(Path.DirectorySeparatorChar + "*"))
            {
                string directory = Path.GetDirectoryName(path);
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

            foreach (string file in Directory.EnumerateFiles(MediaRootFoler, searchPattern, SearchOption.AllDirectories))
            {
                File.Delete(file);
                count++;
            }

            return Task.FromResult(count);

        }

        /// <summary>
        /// 文件是否存在
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<bool> ExistsAsync(string path)
        {
            Check.NotNull(path, nameof(path));

            path = path.NormalizePath();
            return Task.FromResult(File.Exists(Path.Combine(MediaRootFoler, path)));
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<FileSpec> GetFileInfoAsync(string path)
        {
            Check.NotNull(path, nameof(path));

            path = path.NormalizePath();

            var info = new FileInfo(Path.Combine(MediaRootFoler, path));
            if (!info.Exists)
                return Task.FromResult<FileSpec>(null);

            var fileInfo = new FileInfo(Path.Combine(MediaRootFoler, path));

            var fileSpec = new FileSpec(fileInfo.FullName, fileInfo.Length, fileName: fileInfo.Name, System.Guid.NewGuid().ToString())
            {
                SaveName = fileInfo.Name
            };

            return Task.FromResult(fileSpec);
        }

        /// <summary>
        /// 获取文件数据流
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
                return Task.FromResult<Stream>(File.OpenRead(Path.Combine(MediaRootFoler, path)));
            }
            catch (IOException ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                    _logger.LogTrace(ex, "Error trying to get file stream: {Path}", path);

                return Task.FromResult<Stream>(null);
            }
        }

        /// <summary>
        /// 修改文件目录
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
                    string directory = Path.GetDirectoryName(newPath);
                    if (directory != null)
                        Directory.CreateDirectory(Path.Combine(MediaRootFoler, directory));

                    string oldFullPath = Path.Combine(MediaRootFoler, path);
                    string newFullPath = Path.Combine(MediaRootFoler, newPath);
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
                _logger.LogError(ex, "Error trying to rename file {Path} to {NewPath}.", path, newPath);

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// 保存文件
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
            string file = Path.Combine(MediaRootFoler, path);

            try
            {
                using var fileStream = CreateFileStream(file);
                await stream.CopyToAsync(fileStream);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error trying to save file: {Path}", path);
                return false;
            }
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="byteArray"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> SaveFileAsync(string path, byte[] byteArray, CancellationToken cancellationToken = default)
        {
            Check.NotNull(path, nameof(path));
            Check.NotNull(byteArray, nameof(byteArray));

            path = path.NormalizePath();
            string file = Path.Combine(MediaRootFoler, path);

            try
            {
                using var fileStream = CreateFileStream(file);
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
        /// 创建真实文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private Stream CreateFileStream(string filePath)
        {
            try
            {
                return File.Create(filePath);
            }
            catch (DirectoryNotFoundException) { }

            string directory = Path.GetDirectoryName(filePath);
            if (directory != null)
                Directory.CreateDirectory(directory);

            return File.Create(filePath);
        }
    }
}

