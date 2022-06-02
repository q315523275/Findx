using System;
using Findx.Data;

namespace Findx.Module.EleAdmin.DTO
{
	public class QueryMenuRequest: PageBase
	{
		/// <summary>
        /// 名称
        /// </summary>
		public string Title { set; get; }

		/// <summary>
        /// 路径
        /// </summary>
		public string Path { set; get; }

		/// <summary>
        /// 权限标识
        /// </summary>
		public string Authority { set; get; }
	}
}

