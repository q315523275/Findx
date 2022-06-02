using Findx.AspNetCore.Mvc;
using Findx.Module.EleAdmin.DTO;
using Findx.Module.EleAdmin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdmin.Areas.System.Controller
{
	/// <summary>
    /// 字典类型服务
    /// </summary>
	[Area("system")]
	[Route("api/[area]/dictType")]
	[Authorize]
	public class SysDictTypeController : CrudControllerBase<SysDictTypeInfo, SetDictTypeRequest, QueryDictTypeRequest, Guid, Guid>
	{
    }
}

