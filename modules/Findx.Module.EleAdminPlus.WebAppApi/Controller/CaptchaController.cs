using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Module.EleAdminPlus.Shared.ServiceDefaults;
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
    private readonly ICaptchaService _captchaService;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="captchaService"></param>
    public CaptchaController(ICaptchaService captchaService)
    {
        _captchaService = captchaService;
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
        return await _captchaService.GenerateCaptchaAsync(width, height, length, cancellationToken);
    }
}