namespace Findx.Storage;

/// <summary>
///     文件存储提供器
/// </summary>
public class StorageProvider : IStorageProvider
{
    private readonly IConfiguration _settingProvider;
    private readonly IDictionary<string, IFileStorage> _storages;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="storages"></param>
    /// <param name="settingProvider"></param>
    public StorageProvider(IEnumerable<IFileStorage> storages, IConfiguration settingProvider)
    {
        _storages = storages.ToDictionary(it => it.Name, it => it);
        _settingProvider = settingProvider;
    }

    /// <summary>
    ///     获取文件存储器
    /// </summary>
    /// <param name="storageName"></param>
    /// <returns></returns>
    public IFileStorage Get(string storageName = null)
    {
        storageName ??= _settingProvider.GetValue<string>("Findx:Storage:Primary") ?? FileStorageType.Folder.ToString();

        _storages.TryGetValue(storageName, out var storage);

        Check.NotNull(storage, nameof(storage));

        return storage;
    }
}