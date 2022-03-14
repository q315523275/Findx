using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Findx.Module.Admin.Service
{
	/// <summary>
    /// 用户服务
    /// </summary>
	public interface ISysUserService
	{
		Task<List<long>> GetUserDataScopeIdList();

		Task<List<long>> GetUserDataScopeIdList(long userId);
	}
}

