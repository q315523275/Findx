namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 系统用户数据范围表
    /// </summary>
    public partial class SysUserDataScope
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
        /// 用户id
        /// </summary>
        public long UserId { get; set; }

    }

}
