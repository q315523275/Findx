using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findx.Security.Authorization.Store
{
    public class RemotePermissionStore : IPermissionStore
    {
        public PermissionStoreType PermissionStoreType => throw new NotImplementedException();

        public Task<List<Permission>> GetFromStoreAsync()
        {
            throw new NotImplementedException();
        }

        public Task SyncToStoreAsync(List<Permission> permissions)
        {
            throw new NotImplementedException();
        }
    }
}
