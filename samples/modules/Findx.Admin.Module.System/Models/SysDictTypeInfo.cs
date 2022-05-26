using System;
using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Admin.Module.System.Models
{
	/// <summary>
	/// 字典类型
	/// </summary>
	[Table(Name = "sys_dict_type")]
	public class SysDictTypeInfo : EntityBaseFullAudited<int, int>, ISoftDeletable, ISort, IResponse
	{
		/// <summary>
		/// 字典id
		/// </summary>
		[Column(IsPrimary = true, IsIdentity = true)]
		public override int Id { get; set; }

		/// <summary>
		/// 字典标识
		/// </summary>
		public string Code { get; set; }

		/// <summary>
		/// 字典名称
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 排序号
		/// </summary>
		public int Sort { get; set; }

		/// <summary>
		/// 备注
		/// </summary>
		public string Comments { get; set; }

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

