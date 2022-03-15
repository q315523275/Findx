using System;
using Findx.AspNetCore.Mvc;
using Findx.Module.Admin.Cms.DTO;
using Findx.Module.Cms.Models;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Cms.Areas.Cms.Controller
{
	/// <summary>
    /// 信息
    /// </summary>
	[Area("api/cms")]
	[Route("[area]/article")]
	[ApiExplorerSettings(GroupName = "cms")]
	[Authorize(Roles = "admin")]
	public class ArticleController : CrudControllerBase<ArticleInfo, ArticleInfo, ArticleRequest, ArticleRequest, ArticleQuery, long, long>
	{
		
	}
}

