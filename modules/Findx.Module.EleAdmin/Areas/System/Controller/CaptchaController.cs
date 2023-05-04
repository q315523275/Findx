using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Findx.AspNetCore.Mvc;
using Findx.Caching;
using Findx.Data;
using Findx.Drawing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdmin.Areas.System.Controller;

/// <summary>
///     验证码服务
/// </summary>
[Description("系统-验证码")]
[Area("system")]
[Route("api/[area]/captcha")]
[ApiExplorerSettings(GroupName = "eleAdmin")]
[Tags("系统-验证码")]
public class CaptchaController : AreaApiControllerBase
{
    private readonly ICacheProvider _cacheProvider;
    private readonly IVerifyCoder _verifyCoder;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="cacheProvider"></param>
    /// <param name="verifyCoder"></param>
    public CaptchaController(ICacheProvider cacheProvider, IVerifyCoder verifyCoder)
    {
        _cacheProvider = cacheProvider;
        _verifyCoder = verifyCoder;
    }

    /// <summary>
    ///     获取验证码图片
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    [HttpGet("/api/captcha")]
    [Description("获取验证码图片")]
    public async Task<CommonResult> GetCaptcha(int width = 150, int height = 50, [Range(3, 6)] int length = 4)
    {
        var code = _verifyCoder.GetCode(length, VerifyCodeType.NumberAndLetter);
        var st = await _verifyCoder.CreateImageAsync(code, width, height);
        var cache = _cacheProvider.Get();
        var uuid = Guid.NewGuid().ToString("N");
        var cacheKey = $"verifyCode:{uuid}";
        await cache.AddAsync(cacheKey, code.ToLower(), TimeSpan.FromMinutes(2));
        return CommonResult.Success(new
            { text = code.ToLower(), uuid, Base64 = $"data:image/jpeg;base64,{Convert.ToBase64String(st)}" });
    }
}