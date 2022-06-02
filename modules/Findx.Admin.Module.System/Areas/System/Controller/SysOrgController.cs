using System;
using System.Threading.Tasks;
using Findx.Admin.Module.System.DTO;
using Findx.Admin.Module.System.Models;
using Findx.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Admin.Module.System.Areas.System.Controller
{
	/// <summary>
    /// 机构服务
    /// </summary>
	[Area("system")]
	[Route("api/[area]/org")]
	[Authorize]
	public class SysOrgController: CrudControllerBase<SysOrgInfo, SetOrgRequest, QueryOrgRequest, int, int>
	{

	}
}

