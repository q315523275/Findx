using Findx.Extensions;
using Findx.Module.Admin.Models;
using Findx.Security;

namespace Findx.Module.Admin.Internals
{
    /// <summary>
    /// 管理员扩展
    /// </summary>
    public static class AdminExtensions
    {
        /// <summary>
        /// 是否超级管理员
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public static bool IsSuperAdmin(this SysUserInfo userInfo)
        {
            return AdminTypeEnum.SuperAdmin.To<int>() == userInfo.AdminType;
        }

        /// <summary>
        /// 是否超级管理员
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
		public static bool IsSuperAdmin(this ICurrentUser user)
        {
            return user.FindClaim("SuperAdmin")?.Value == ((int)AdminTypeEnum.SuperAdmin).ToString();
        }
    }
}
