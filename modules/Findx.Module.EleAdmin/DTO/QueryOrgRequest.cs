using System;
using Findx.Data;

namespace Findx.Module.EleAdmin.DTO
{
	/// <summary>
	/// 查询组织入参
	/// </summary>
	public class QueryOrgRequest: PageBase
	{
		/// <summary>
		/// 父级组织Id
		/// </summary>
		public Guid? Pid { set; get; }
		
		/// <summary>
		/// 关键词
		/// </summary>
		public string Keywords { set; get; }
	}
}

