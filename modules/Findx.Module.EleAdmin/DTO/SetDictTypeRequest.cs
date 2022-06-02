using System;
using Findx.Data;

namespace Findx.Module.EleAdmin.DTO
{
	public class SetDictTypeRequest: IRequest
	{
		public int Id { get; set; }

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
	}
}

