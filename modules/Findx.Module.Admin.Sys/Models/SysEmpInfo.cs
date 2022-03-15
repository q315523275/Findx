using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.Admin.Models
{
    /// <summary>
    /// 员工表
    /// </summary>
    [Table(Name = "sys_emp")]
    public class SysEmpInfo : EntityBase<long>, IResponse, IRequest
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Column(Name = "id", IsPrimary = true)]
        public override long Id { get; set; }

        /// <summary>
        /// 工号
        /// </summary>
        [Column(Name = "job_num", DbType = "varchar(100)")]
        public string JobNum { get; set; }

        /// <summary>
        /// 所属机构id
        /// </summary>
        [Column(Name = "org_id")]
        public long OrgId { get; set; }

        /// <summary>
        /// 所属机构名称
        /// </summary>
        [Column(Name = "org_name", DbType = "varchar(100)")]
        public string OrgName { get; set; }
    }
}
