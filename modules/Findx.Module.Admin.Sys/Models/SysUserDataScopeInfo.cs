using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.Admin.Models
{
    /// <summary>
    /// 系统用户数据范围表
    /// </summary>
    [Table(Name = "sys_user_data_scope")]
    public class SysUserDataScopeInfo : EntityBase<long>, IResponse, IRequest
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
        /// 用户id
        /// </summary>
        [Column(Name = "user_id")]
        public long UserId { get; set; }
    }
}
