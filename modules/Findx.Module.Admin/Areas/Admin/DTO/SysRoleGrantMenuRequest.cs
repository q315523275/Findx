using System;
using System.Collections.Generic;
using Findx.Data;

namespace Findx.Module.Admin.Areas.Admin.DTO
{
	public class SysRoleGrantMenuRequest: IRequest
	{
		/// <summary>
        /// 角色编号
        /// </summary>
		public long Id { set; get; }

		/// <summary>
        /// 菜单编号集合
        /// </summary>
		public List<long> GrantMenuIdList { set; get; }
	}
}

