namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 员工附属机构岗位表
    /// </summary>
    public partial class SysEmpExtOrgPos
    {

        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 员工id
        /// </summary>
        public long EmpId { get; set; }

        /// <summary>
        /// 机构id
        /// </summary>
        public long OrgId { get; set; }

        /// <summary>
        /// 岗位id
        /// </summary>
        public long PosId { get; set; }

    }

}
