using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.Admin.Models
{
    /// <summary>
    /// 系统角色数据范围表
    /// </summary>
    [Table(Name = "sys_role_data_scope")]
    public class SysRoleDataScopeInfo : EntityBase<long>, IResponse, IRequest
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Column(Name = "id", IsPrimary = true)]
        public override long Id { get; set; }

        /// <summary>
        /// 机构id
        /// </summary>
        [Column(Name = "org_id")]
        public long OrgId { get; set; }

        /// <summary>
        /// 角色id
        /// </summary>
        [Column(Name = "role_id")]
        public long RoleId { get; set; }
    }
}
