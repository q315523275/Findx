using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Findx.AspNetCore.Mvc;
using Findx.Caching;
using Findx.Data;
using Findx.Imaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdminPlus.WebAppApi.Controller;

/// <summary>
///     验证码服务
/// </summary>
[Area("system")]
[Route("api/[area]/captcha")]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-验证码"), Description("系统-验证码")]
public class CaptchaController : AreaApiControllerBase
{
    private readonly ICacheFactory _cacheFactory;
    private readonly IVerifyCoder _verifyCoder;
    private readonly IKeyGenerator<Guid> _keyGenerator;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="verifyCoder"></param>
    /// <param name="cacheFactory"></param>
    /// <param name="keyGenerator"></param>
    public CaptchaController(IVerifyCoder verifyCoder, ICacheFactory cacheFactory, IKeyGenerator<Guid> keyGenerator)
    {
        _verifyCoder = verifyCoder;
        _cacheFactory = cacheFactory;
        _keyGenerator = keyGenerator;
    }

    /// <summary>
    ///     获取验证码图片
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="length"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/captcha"), Description("获取验证码图片")]
    [DisableAuditing]
    public async Task<CommonResult> CaptchaAsync(int width = 150, int height = 50, [Range(3, 6)] int length = 4, CancellationToken cancellationToken = default)
    {
        var code = _verifyCoder.GetCode(length, VerifyCodeType.NumberAndLetter);
        var st = await _verifyCoder.CreateImageAsync(code, width, height, 38);
        var uuid = _keyGenerator.Create();
        var cacheKey = $"verifyCode:{uuid}";
        var cache = _cacheFactory.Create(CacheType.DefaultMemory);
        await cache.AddAsync(cacheKey, code.ToLower(), TimeSpan.FromMinutes(2), cancellationToken);
        return CommonResult.Success(new { text = code.ToLower(), uuid, Base64 = $"data:image/png;base64,{Convert.ToBase64String(st)}" });
    }
}