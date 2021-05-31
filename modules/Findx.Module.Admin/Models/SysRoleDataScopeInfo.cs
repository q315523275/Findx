namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 系统角色数据范围表
    /// </summary>
    public partial class SysRoleDataScope
    {

        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 机构id
        /// </summary>
        public long OrgId { get; set; }

        /// <summary>
        /// 角色id
        /// </summary>
        public long RoleId { get; set; }

    }

}
