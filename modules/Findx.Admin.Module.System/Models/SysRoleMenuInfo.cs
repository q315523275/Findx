using System;
using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Admin.Module.System.Models
{
	/// <summary>
    /// 角色菜单
    /// </summary>
	[Table(Name = "sys_role_menu")]
	public class SysRoleMenuInfo : EntityBase<int>
	{
		/// <summary>
		/// 主键id
		/// </summary>
		[Column(IsPrimary = true, IsIdentity = true)]
		public override int Id { get; set; }

		/// <summary>
		/// 角色id
		/// </summary>
		public int RoleId { get; set; }

		/// <summary>
		/// 菜单id
		/// </summary>
		public int MenuId { get; set; }

		/// <summary>
		/// 租户id
		/// </summary>
		public int TenantId { get; set; }

		/// <summary>
		/// 角色信息
		/// </summary>
		[Navigate(nameof(MenuId))]
		public virtual SysMenuInfo MenuInfo { set; get; }
	}
}

