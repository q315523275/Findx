using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Drawing;
using Findx.Module.Admin.Captcha;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Areas.Sys.Controllers
{
    /// <summary>
    /// 验证码
    /// </summary>
    [Area("api/sys")]
	[Route("[area]/captcha")]
	[ApiExplorerSettings(GroupName = "system")]
	public class CaptchaController : AreaApiControllerBase
	{
		private readonly IClickWordCaptcha _captchaHandle; // 验证码服务

		/// <summary>
        /// Ctor
        /// </summary>
        /// <param name="captchaHandle"></param>
        public CaptchaController(IClickWordCaptcha captchaHandle)
        {
            _captchaHandle = captchaHandle;
        }

        /// <summary>
        /// 是否开启验证码验证
        /// </summary>
        /// <returns></returns>
        [HttpGet("getCaptchaOpen")]
		public CommonResult GetCaptchaOpen()
        {
			return CommonResult.Success(true);
        }

		/// <summary>
		/// 验证码形式
		/// </summary>
		/// <returns></returns>
		[HttpPost("get")]
		public async Task<ClickWordCaptchaResult> GetCaptchaType()
		{
			// 图片大小要与前端保持一致（坐标范围）
			return await _captchaHandle.CreateCaptchaImage(_captchaHandle.RandomCode(4), 310, 155);
		}

		/// <summary>
		/// 校验验证码
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		[HttpPost("check")]
		public async Task<ClickWordCaptchaResult> VerificationCode(ClickWordCaptchaRequest input)
		{
			return await _captchaHandle.CheckCode(input);
		}

		/// <summary>
		/// 短信验证码图片
		/// </summary>
		/// <returns></returns>
		[HttpGet("getSmsCaptcha")]
		public CommonResult GetSmsCaptcha([Required, Phone] string mobile)
		{
			return CommonResult.Fail("401", "暂时不提供短信服务");
		}

		/// <summary>
		/// 图片验证码
		/// </summary>
		/// <param name="verifyCoder"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		[HttpGet("getImgCaptcha")]
		public async Task<IActionResult> VerifyCode([FromServices] IVerifyCoder verifyCoder, int width = 150, int height = 50, [Range(4, 6)] int count = 4)
		{
			var code = verifyCoder.GetCode(count, VerifyCodeType.NumberAndLetter);

			var img = await verifyCoder.CreateImageAsync(code, width: width, height: height);

			return File(img, "image/jpeg");
		}
	}
}

