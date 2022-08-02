using System;
using Findx.Data;

namespace Findx.Module.EleAdmin.DTO
{
	/// <summary>
	/// 查询菜单入参
	/// </summary>
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
		
		/// <summary>
		/// 父级id
		/// </summary>
		public Guid? ParentId { set; get; }

        /// <summary>
        /// 应用名称
        /// </summary>
        public string ApplicationCode { get; set; }
    }
}

