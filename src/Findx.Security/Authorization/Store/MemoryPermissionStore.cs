using System.Collections.Generic;
using System.Threading.Tasks;

namespace Findx.Security.Authorization.Store
{
    public class MemoryPermissionStore : IPermissionStore
    {
        public PermissionStoreType PermissionStoreType => PermissionStoreType.Memory;

        private List<Permission> _permissions;
        public MemoryPermissionStore()
        {
            _permissions = new List<Permission>();
        }

        public Task<List<Permission>> GetFromStoreAsync()
        {
            return Task.FromResult(_permissions);
        }

        public Task SyncToStoreAsync(List<Permission> permissions)
        {
            _permissions.Clear();
            _permissions.AddRange(permissions);
            return Task.CompletedTask;
        }
    }
}
