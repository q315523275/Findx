using System.Security.Principal;
using System.Threading.Tasks;

namespace Findx.Security.Authorization
{
    /// <summary>
    /// 权限检测验证
    /// </summary>
    public interface IPermissionChecker
    {
        /// <summary>
        /// 是否授权
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="permissionAccess"></param>
        /// <returns></returns>
        Task<bool> IsGrantedAsync(IPrincipal principal, PermissionAccess permissionAccess);
    }
}
