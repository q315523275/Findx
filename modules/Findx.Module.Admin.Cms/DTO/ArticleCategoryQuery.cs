using System;
using Findx.Data;

namespace Findx.Module.Admin.Cms.DTO
{
	public class ArticleCategoryQuery: PageBase
	{
		/// <summary>
        /// 频道编号
        /// </summary>
		public int? ChannelId { set; get; }

		/// <summary>
        /// 状态
        /// </summary>
		public int? Status { set; get; }
	}
}

