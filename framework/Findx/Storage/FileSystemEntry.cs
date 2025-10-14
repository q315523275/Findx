using Microsoft.Extensions.FileProviders;

namespace Findx.Storage;

/// <summary>
///     文件系统条目数据
/// </summary>
public class FileSystemEntry: IFileEntry
{
    private readonly IFileInfo _fileInfo;
    private readonly string _path;

    internal FileSystemEntry(string path, IFileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(fileInfo);

        _fileInfo = fileInfo;
        _path = path;
    }
    
    /// <summary>
    ///     文件完整路径
    /// </summary>
    public string Path => _path;
    
    /// <summary>
    ///     文件名称
    /// </summary>
    public string Name => _fileInfo.Name;
    
    /// <summary>
    ///     文件所在文件夹目录
    /// </summary>
    public string DirectoryPath => _path[..^Name.Length].TrimEnd('/');
    
    /// <summary>
    ///     最后修改时间
    /// </summary>
    public DateTime LastModifiedTime => _fileInfo.LastModified.DateTime;
    
    /// <summary>
    ///     文件大小
    /// </summary>
    public long Length => _fileInfo.Length;
    
    /// <summary>
    ///     是否目录
    /// </summary>
    public bool IsDirectory => _fileInfo.IsDirectory;
}