using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdmin.Models
{
	/// <summary>
    /// 字典值
    /// </summary>
    [Table(Name = "sys_dict_data")]
	[DataEntity(DataSource = "system")]
	public class SysDictDataInfo : FullAuditedBase<Guid, Guid>, ISoftDeletable, ISort, IResponse
	{
		/// <summary>
        /// 字典项id
        /// </summary>
        [Column(IsPrimary = true, IsIdentity = true)]
		public override Guid Id { get; set; }

		/// <summary>
		/// 字典id
		/// </summary>
		public Guid TypeId { get; set; }
		
		/// <summary>
		/// 字典项名称
		/// </summary>
		public string Name { get; set; }
		
		/// <summary>
		/// 字典项值
		/// </summary>
		public string Value { get; set; }

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
