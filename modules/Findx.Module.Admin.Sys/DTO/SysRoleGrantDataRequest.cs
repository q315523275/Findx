﻿using System;
using System.Collections.Generic;
using Findx.Data;

namespace Findx.Module.Admin.Sys.DTO
{
	public class SysRoleGrantDataRequest: IRequest
	{
		/// <summary>
        /// 角色编号
        /// </summary>
		public long Id { set; get; }

		/// <summary>
        /// 数据范围类型
        /// </summary>
		public int DataScopeType { set; get; }

		/// <summary>
		/// 菜单编号集合
		/// </summary>
		public List<long> GrantOrgIdList { set; get; }
	}
}

