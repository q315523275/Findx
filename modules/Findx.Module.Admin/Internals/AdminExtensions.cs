using Findx.Extensions;
using Findx.Module.Admin.Models;
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
            return AdminTypeEnum.SUPER_ADMIN.To<int>() == userInfo.AdminType;
        }
    }
}
