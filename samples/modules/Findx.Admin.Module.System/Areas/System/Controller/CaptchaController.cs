using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Findx.AspNetCore.Mvc;
using Findx.Caching;
using Findx.Data;
using Findx.Drawing;
using Findx.Setting;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Admin.Module.System.Areas.System.Controller
{
    /// <summary>
    /// 验证码服务
    /// </summary>
    [Area("system")]
	[Route("api/[area]/captcha")]
	public class CaptchaController : AreaApiControllerBase
	{
		private readonly ICacheProvider _cacheProvider;
		private readonly IVerifyCoder _verifyCoder;
		private readonly IApplicationContext _applicationContext;
        private readonly ISettingProvider _setting;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="cacheProvider"></param>
        /// <param name="verifyCoder"></param>
        /// <param name="applicationContext"></param>
        public CaptchaController(ISettingProvider setting, ICacheProvider cacheProvider, IVerifyCoder verifyCoder, IApplicationContext applicationContext)
        {
            _setting = setting;
            _cacheProvider = cacheProvider;
            _verifyCoder = verifyCoder;
            _applicationContext = applicationContext;
        }

        /// <summary>
        /// 获取验证码图片
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        [HttpGet("/api/captcha")]
        public async Task<CommonResult> GetCaptcha(int width = 150, int height = 50)
        {
            var code = _verifyCoder.GetCode(4, VerifyCodeType.NumberAndLetter);
            var st = await _verifyCoder.CreateImageAsync(code, width, height);
            var cache = _cacheProvider.Get();
            var uuid = Guid.NewGuid().ToString();
            cache.Add($"verifyCode:" + uuid, code.ToLower(), TimeSpan.FromMinutes(2));
            // return File(st, "image/jpeg");
            return CommonResult.Success(new { text = code.ToLower(), uuid, Base64 = $"data:image/jpeg;base64,{Convert.ToBase64String(st)}" });
        }
    }
}

