using System;
using System.Collections.Generic;
using Findx.Data;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Sys.Areas.Sys.Controllers
{
	/// <summary>
    /// 在线
    /// </summary>
	[Area("api/sys")]
	[Route("[area]/sysOnlineUser")]
	[ApiExplorerSettings(GroupName = "system")]
	[Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
	public class SysOnlineUserController: ControllerBase
	{
		/// <summary>
        /// 列表
        /// </summary>
        /// <returns></returns>
		[HttpGet("list")]
		public CommonResult List()
		{
			return CommonResult.Success(new PageResult<List<string>>(1, 20, 0, null));
		}
	}
}

