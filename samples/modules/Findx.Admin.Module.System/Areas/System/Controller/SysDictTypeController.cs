using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Findx.Admin.Module.System.DTO;
using Findx.Admin.Module.System.Models;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Admin.Module.System.Areas.System.Controller
{
	/// <summary>
    /// 字典类型服务
    /// </summary>
	[Area("system")]
	[Route("api/[area]/dictType")]
	[Authorize]
	public class SysDictTypeController : CrudControllerBase<SysDictTypeInfo, SetDictTypeRequest, QueryDictTypeRequest, int, int>
	{
    }
}

