using System;
using Findx.Data;

namespace Findx.Module.Admin.Cms.DTO
{
	public class ArticleQuery: PageBase
	{
        /// <summary>
        /// 查询关键词
        /// </summary>
        public string SearchValue { get; set; }

        /// <summary>
        /// 查询状态
        /// </summary>
        public int SearchStatus { get; set; } = -1;

        /// <summary>
        /// 类别编号
        /// </summary>
        public string CategoryId { get; set; }
    }
}

