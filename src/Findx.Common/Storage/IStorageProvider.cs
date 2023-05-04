namespace Findx.Storage;

/// <summary>
///     文件存储提供器
/// </summary>
public interface IStorageProvider
{
    /// <summary>
    ///     获取文件存储器
    /// </summary>
    /// <param name="storageName"></param>
    /// <returns></returns>
    IFileStorage Get(string storageName = null);
}