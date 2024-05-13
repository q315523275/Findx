using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Findx.AspNetCore.Mvc;
using Findx.Caching;
using Findx.Data;
using Findx.Imaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdmin.Controller;

/// <summary>
///     验证码服务
/// </summary>
[Area("system")]
[Route("api/[area]/captcha")]
[ApiExplorerSettings(GroupName = "eleAdmin"), Tags("系统-验证码"), Description("系统-验证码")]
public class CaptchaController : AreaApiControllerBase
{
    private readonly ICacheFactory _cacheFactory;
    private readonly IVerifyCoder _verifyCoder;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="verifyCoder"></param>
    /// <param name="cacheFactory"></param>
    public CaptchaController(IVerifyCoder verifyCoder, ICacheFactory cacheFactory)
    {
        _verifyCoder = verifyCoder;
        _cacheFactory = cacheFactory;
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
    public async Task<CommonResult> CaptchaAsync(int width = 150, int height = 50, [Range(3, 6)] int length = 4)
    {
        var code = _verifyCoder.GetCode(length, VerifyCodeType.NumberAndLetter);
        var st = await _verifyCoder.CreateImageAsync(code, width, height, 38);
        var cache = _cacheFactory.Create(CacheType.DefaultMemory);
        var uuid = Guid.NewGuid().ToString("N");
        var cacheKey = $"verifyCode:{uuid}";
        await cache.AddAsync(cacheKey, code.ToLower(), TimeSpan.FromMinutes(2));
        return CommonResult.Success(new
            { text = code.ToLower(), uuid, Base64 = $"data:image/png;base64,{Convert.ToBase64String(st)}" });
    }
}