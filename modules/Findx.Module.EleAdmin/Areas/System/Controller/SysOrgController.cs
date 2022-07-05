using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Module.EleAdmin.DTO;
using Findx.Module.EleAdmin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdmin.Areas.System.Controller
{
	/// <summary>
    /// 机构服务
    /// </summary>
	[Area("system")]
	[Route("api/[area]/org")]
	[Authorize]
	[Description("系统-机构")]
	public class SysOrgController: CrudControllerBase<SysOrgInfo, SetOrgRequest, QueryOrgRequest, Guid, Guid>
	{

	}
}

