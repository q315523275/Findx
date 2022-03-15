using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.Admin.Models
{
    /// <summary>
    /// 系统用户角色表
    /// </summary>
    [Table(Name = "sys_user_role")]
    public class SysUserRoleInfo : EntityBase<long>, IResponse, IRequest
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Column(Name = "id", IsPrimary = true)]
        public override long Id { get; set; }

        /// <summary>
        /// 角色id
        /// </summary>
        [Column(Name = "role_id")]
        public long RoleId { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        [Column(Name = "user_id")]
        public long UserId { get; set; }
    }
}
