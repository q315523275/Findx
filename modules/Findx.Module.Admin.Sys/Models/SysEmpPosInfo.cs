using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.Admin.Models
{
    /// <summary>
    /// 员工职位关联表
    /// </summary>
    [Table(Name = "sys_emp_pos")]
    public class SysEmpPosInfo : EntityBase<long>, IResponse, IRequest
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
        /// 职位id
        /// </summary>
        [Column(Name = "pos_id")]
        public long PosId { get; set; }
    }
}
