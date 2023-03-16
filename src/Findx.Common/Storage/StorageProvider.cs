using Findx.Setting;

namespace Findx.Storage
{
    /// <summary>
    /// 文件存储提供器
    /// </summary>
    public class StorageProvider : IStorageProvider
    {
        private readonly IDictionary<string, IFileStorage> _storages;

        private readonly ISettingProviderFactory _settingFactory;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="storages"></param>
        /// <param name="settingFactory"></param>
        public StorageProvider(IEnumerable<IFileStorage> storages, ISettingProviderFactory settingFactory)
        {
            _storages = storages.ToDictionary(it => it.Name, it => it);
            _settingFactory = settingFactory;
        }

        /// <summary>
        /// 获取文件存储器
        /// </summary>
        /// <param name="storageName"></param>
        /// <returns></returns>
        public IFileStorage Get(string storageName = null)
        {
            storageName ??= _settingFactory.Get().GetValue<string>("Findx:Storage:Primary") ?? FileStorageType.Folder.ToString();

            _storages.TryGetValue(storageName, out var storage);

            Check.NotNull(storage, nameof(storage));

            return storage;
        }
    }
}
