using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.Admin.Models
{
    /// <summary>
    /// 员工附属机构岗位表
    /// </summary>
    [Table(Name = "sys_emp_ext_org_pos")]
    public class SysEmpExtOrgPosInfo : EntityBase<long>, IResponse, IRequest
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Column(Name = "id", IsPrimary = true)]
        public override long Id { get; set; }

        /// <summary>
        /// 员工id
        /// </summary>
        [Column(Name = "emp_id")]
        public long EmpId { get; set; }

        /// <summary>
        /// 机构id
        /// </summary>
        [Column(Name = "org_id")]
        public long OrgId { get; set; }

        /// <summary>
        /// 岗位id
        /// </summary>
        [Column(Name = "pos_id")]
        public long PosId { get; set; }
    }
}
