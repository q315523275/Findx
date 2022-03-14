using System;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
	/// <summary>
	/// 验证码
	/// </summary>
	[Area("api/admin")]
	[Route("[area]/captcha")]
	public class CaptchaController : AreaApiControllerBase
	{
		/// <summary>
        /// 是否开启验证码验证
        /// </summary>
        /// <returns></returns>
		[HttpGet("getCaptchaOpen")]
		public CommonResult GetCaptchaOpen()
        {
			return CommonResult.Success(false);
        }

		/// <summary>
		/// 验证码形式
		/// </summary>
		/// <returns></returns>
		[HttpPost("get")]
		public CommonResult GetCaptchaType()
		{
			return CommonResult.Success("");
		}
	}
}

