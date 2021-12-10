using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.Admin.Models
{
    /// <summary>
    /// 系统角色菜单表
    /// </summary>
    [Table(Name = "sys_role_menu")]
    public class SysRoleMenuInfo : EntityBase<long>, IResponse, IRequest
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Column(Name = "id", IsPrimary = true)]
        public override long Id { get; set; }

        /// <summary>
        /// 菜单id
        /// </summary>
        [Column(Name = "menu_id")]
        public long MenuId { get; set; }

        /// <summary>
        /// 角色id
        /// </summary>
        [Column(Name = "role_id")]
        public long RoleId { get; set; }
    }
}
