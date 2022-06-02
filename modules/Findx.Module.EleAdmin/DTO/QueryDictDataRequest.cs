using System;
using Findx.Data;

namespace Findx.Module.EleAdmin.DTO
{
	public class QueryDictDataRequest: PageBase
	{
		/// <summary>
        /// TypeId
        /// </summary>
		public Guid? TypeId { set; get; }
		
		/// <summary>
		/// 字典编号
		/// </summary>
		public string TypeCode { set; get; }

		/// <summary>
        /// 关键字
        /// </summary>
		public string Keywords { set; get; }
	}
}

