namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 员工表
    /// </summary>
    public partial class SysEmp
    {

        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 工号
        /// </summary>
        public string JobNum { get; set; } = string.Empty;

        /// <summary>
        /// 所属机构id
        /// </summary>
        public long OrgId { get; set; }

        /// <summary>
        /// 所属机构名称
        /// </summary>
        public string OrgName { get; set; } = string.Empty;

    }

}
