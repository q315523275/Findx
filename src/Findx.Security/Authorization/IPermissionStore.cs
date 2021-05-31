using System.Collections.Generic;
using System.Threading.Tasks;

namespace Findx.Security.Authorization
{
    public interface IPermissionStore
    {
        PermissionStoreType PermissionStoreType { get; }
        Task<List<Permission>> GetFromStoreAsync();
        Task SyncToStoreAsync(List<Permission> permissions);
    }
}
