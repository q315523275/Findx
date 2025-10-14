namespace Findx.Storage;

/// <summary>
///     文件接口
/// </summary>
public interface IFileEntry
{
    /// <summary>
    ///     获取该文件在文件存储中的完整路径
    /// </summary>
    string Path { get; }

    /// <summary>
    ///     获取该文件的名称
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     获取该文件所在目录在文件存储中的完整路径
    /// </summary>
    string DirectoryPath { get; }

    /// <summary>
    ///     获取文件的长度（如果是目录，则为0）
    /// </summary>
    long Length { get; }

    /// <summary>
    ///     获取该文件存储最后修改的时间。
    /// </summary>
    DateTime LastModifiedTime { get; }

    /// <summary>
    ///     获取该文件是否为目录
    /// </summary>
    bool IsDirectory { get; }
}