namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 系统角色菜单表
    /// </summary>
    public partial class SysRoleMenu
    {

        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 菜单id
        /// </summary>
        public long MenuId { get; set; }

        /// <summary>
        /// 角色id
        /// </summary>
        public long RoleId { get; set; }

    }

}
