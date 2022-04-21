using System.ComponentModel;

namespace Findx.Security.Authorization
{
    /// <summary>
    /// 权限访问信息
    /// </summary>
    public class PermissionAccess
    {
        public PermissionAccess(PermiessionAccessType accessType, string[] roles)
        {
            AccessType = accessType;
            Roles = roles;
        }

        /// <summary>
        /// 访问类型
        /// </summary>
        [DisplayName("访问类型")]
        public PermiessionAccessType AccessType { get; set; }

        /// <summary>
        /// 访问角色
        /// </summary>
        [DisplayName("访问角色")]
        public string[] Roles { get; set; }
    }
}
