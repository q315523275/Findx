using System;
using Findx.Data;

namespace Findx.Admin.Module.System.DTO
{
	public class SetDictDataRequest: IRequest
	{
		public int Id { get; set; }

		/// <summary>
		/// 字典id
		/// </summary>
		public int DictId { get; set; }

		/// <summary>
		/// 字典项标识
		/// </summary>
		public string Code { get; set; }

		/// <summary>
		/// 字典项名称
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
	}
}

