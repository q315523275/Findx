using System;
using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Admin.Module.System.Models
{
	/// <summary>
	/// 角色
	/// </summary>
	[Table(Name = "sys_role")]
	public class SysRoleInfo : EntityBaseFullAudited<int, int>, ISoftDeletable, ITenant, IResponse
	{
		/// <summary>
		/// 角色id
		/// </summary>
		[Column(IsPrimary = true, IsIdentity = true)]
		public override int Id { get; set; }

		/// <summary>
		/// 角色名称
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 角色标识
		/// </summary>
		public string Code { get; set; }

		/// <summary>
		/// 备注
		/// </summary>
		public string Comments { get; set; }

		/// <summary>
		/// 租户id
		/// </summary>
		public int? TenantId { get; set; }

		/// <summary>
		/// 是否删除
		/// </summary>
		public bool IsDeleted { get; set; }

		/// <summary>
		/// 删除时间
		/// </summary>
		public DateTime? DeletionTime { get; set; }
	}
}

