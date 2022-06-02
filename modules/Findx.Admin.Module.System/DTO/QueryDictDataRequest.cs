using System;
using Findx.Data;

namespace Findx.Admin.Module.System.DTO
{
	public class QueryDictDataRequest: PageBase
	{
		/// <summary>
        /// TypeId
        /// </summary>
		public int DictId { set; get; }

		/// <summary>
        /// 关键字
        /// </summary>
		public string Keywords { set; get; }

		/// <summary>
        /// 字典编号
        /// </summary>
		public string DictCode { set; get; }
	}
}

