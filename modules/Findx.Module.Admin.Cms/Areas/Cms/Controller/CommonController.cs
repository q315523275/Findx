using System;
using Findx.Module.Cms.Models;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Cms.Areas.Cms.Controller
{
	public class CommonController: ControllerBase
	{
		[HttpGet("/cms/initTable")]
		public string InitTable([FromServices] IFreeSql fsql)
        {
			fsql.CodeFirst.SyncStructure(typeof(ArticleInfo), typeof(ArticleCategoryInfo));

			return DateTime.Now.ToString();
        }
	}
}

