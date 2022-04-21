using System.Collections.Generic;
using System.Linq;
using Findx.Setting;

namespace Findx.Storage
{
    public class StorageProvider : IStorageProvider
    {
        private readonly IDictionary<string, IFileStorage> _storages;

        private readonly ISettingProvider _setting;

        public StorageProvider(IEnumerable<IFileStorage> storages, ISettingProvider setting)
        {
            _storages = storages.ToDictionary(it => it.Name, it => it);
            _setting = setting;
        }

        public IFileStorage Get(string storageName = null)
        {
            storageName ??= _setting.GetValue<string>("Storage:Primary") ?? FileStorageType.Folder.ToString();

            _storages.TryGetValue(storageName, out IFileStorage storage);

            Check.NotNull(storage, nameof(storage));

            return storage;
        }
    }
}
