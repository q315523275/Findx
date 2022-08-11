using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdmin.Models
{
	/// <summary>
	/// 系统用户角色
	/// </summary>
	[Table(Name = "sys_user_role")]
	[EntityExtension(DataSource = "system")]
	public class SysUserRoleInfo : EntityBase<Guid>, ITenant
	{
		/// <summary>
		/// 主键id
		/// </summary>
		[Column(IsPrimary = true, IsIdentity = true)]
		public override Guid Id { get; set; }

		/// <summary>
		/// 用户id
		/// </summary>
		public Guid UserId { get; set; }

		/// <summary>
		/// 角色id
		/// </summary>
		public Guid RoleId { get; set; }

		/// <summary>
		/// 租户id
		/// </summary>
		public Guid? TenantId { get; set; }

		/// <summary>
        /// 角色信息
        /// </summary>
		[Navigate(nameof(RoleId))]
		public virtual SysRoleInfo RoleInfo { set; get; }
	}
}

