using System;
using System.Collections.Generic;
using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Admin.Module.System.Models
{
	/// <summary>
	/// 系统用户角色
	/// </summary>
	[Table(Name = "sys_user_role")]
	public class SysUserRoleInfo : EntityBase<int>, ITenant
	{
		/// <summary>
		/// 主键id
		/// </summary>
		[Column(IsPrimary = true, IsIdentity = true)]
		public override int Id { get; set; }

		/// <summary>
		/// 用户id
		/// </summary>
		public int UserId { get; set; }

		/// <summary>
		/// 角色id
		/// </summary>
		public int RoleId { get; set; }

		/// <summary>
		/// 租户id
		/// </summary>
		public int? TenantId { get; set; }

		/// <summary>
        /// 角色信息
        /// </summary>
		[Navigate(nameof(RoleId))]
		public virtual SysRoleInfo RoleInfo { set; get; }
	}
}

