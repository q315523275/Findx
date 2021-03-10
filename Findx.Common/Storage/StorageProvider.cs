using System.Collections.Generic;
using System.Linq;
namespace Findx.Storage
{
    public class StorageProvider : IStorageProvider
    {
        private readonly IDictionary<string, IStorage> _storages;

        public StorageProvider(IEnumerable<IStorage> storages)
        {
            _storages = storages.ToDictionary(it => it.StorageName, it => it);
        }

        public IStorage Get(string storageName = "Local")
        {
            Check.NotNullOrWhiteSpace(storageName, nameof(storageName));

            _storages.TryGetValue(storageName, out IStorage storage);

            Check.NotNull(storage, nameof(storage));

            return storage;
        }
    }
}
