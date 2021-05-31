namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 系统用户角色表
    /// </summary>
    public partial class SysUserRole
    {

        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 角色id
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public long UserId { get; set; }

    }

}
