using Findx.Extensions;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Findx.Security.Authorization
{
    /// <summary>
    /// 权限检测验证器
    /// </summary>
    public class PermissionChecker : IPermissionChecker
    {
        public PermissionChecker(IOptions<AuthorizationOptions> options)
        {
            SuperRoleName = options.Value?.SuperRoleName;
        }

        /// <summary>
        /// 超级管理员名
        /// </summary>
        private string SuperRoleName { get; }

        /// <summary>
        /// 是否授权验证
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="permissionAccess"></param>
        /// <returns></returns>
        public Task<bool> IsGrantedAsync(IPrincipal principal, PermissionAccess permissionAccess)
        {
            // 未登录
            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                return Task.FromResult(false);
            }
            // 已登录，无角色限制
            if (permissionAccess.AccessType == PermiessionAccessType.Login)
            {
                return Task.FromResult(true);
            }
            // 检查角色-功能的权限
            var userRoleNames = principal.Identity.GetRoles();
            // 超级管理员
            if (!SuperRoleName.IsNullOrWhiteSpace() && userRoleNames.Contains(SuperRoleName))
            {
                return Task.FromResult(true);
            }
            // 角色交集验证
            if (permissionAccess.Roles.Length == 0 || userRoleNames.Intersect(permissionAccess.Roles).Any())
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
