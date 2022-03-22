using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Findx.Module.Admin.Sys.DTO;

namespace Findx.Module.Admin.Sys.Service
{
	/// <summary>
    /// 用户服务
    /// </summary>
	public interface ISysUserService
	{
		Task<List<long>> GetUserDataScopeIdList(long userId);
	}
}

