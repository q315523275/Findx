namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 员工职位关联表
    /// </summary>
    public partial class SysEmpPos
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
        /// 职位id
        /// </summary>
        public long PosId { get; set; }

    }

}
