using System;
using Findx.Data;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers
{
	public abstract class PermissionControllerBase: ControllerBase
	{


		[HttpGet("hi")]
		public virtual CommonResult hi()
        {
			return CommonResult.Success(DateTime.Now.ToString());
        }
	}
}

