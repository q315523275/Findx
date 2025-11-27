using Findx.Caching;
using Findx.Data;
using Findx.DependencyInjection;
using Findx.Imaging;
using Findx.Module.EleAdminPlus.Shared.ServiceDefaults;
using Findx.NewId;
using Microsoft.Extensions.Hosting;

namespace Findx.Module.EleAdminPlus.ServiceDefaults;

/// <summary>
///     验证码服务实现
/// </summary>
public class CaptchaService : ICaptchaService, IScopeDependency
{
    private readonly ICacheFactory _cacheFactory;
    private readonly IVerifyCoder _verifyCoder;
    private readonly IKeyGenerator<Guid> _keyGenerator;
    private readonly IApplicationContext _applicationContext;

    /// <summary>
    ///     Ctor
    /// </summary>
    public CaptchaService(IVerifyCoder verifyCoder, ICacheFactory cacheFactory, IKeyGenerator<Guid> keyGenerator, IApplicationContext applicationContext)
    {
        _verifyCoder = verifyCoder;
        _cacheFactory = cacheFactory;
        _keyGenerator = keyGenerator;
        _applicationContext = applicationContext;
    }

    /// <summary>
    ///     生成验证码
    /// </summary>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <param name="length">长度</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>验证码结果</returns>
    public async Task<CommonResult> GenerateCaptchaAsync(int width, int height, int length, CancellationToken cancellationToken = default)
    {
        var code = _verifyCoder.GetCode(length, VerifyCodeType.NumberAndLetter);
        var st = await _verifyCoder.CreateImageAsync(code, width, height, 38);
        var uuid = _keyGenerator.Create();
        var cacheKey = $"verifyCode:{uuid}";
        var cache = _cacheFactory.Create(CacheType.DefaultMemory);
        await cache.AddAsync(cacheKey, code.ToLower(), TimeSpan.FromMinutes(2), cancellationToken);
        code = _applicationContext.HostEnvironment.IsProduction() ? null : code.ToLower();
        return CommonResult.Success(new { text = code, uuid, Base64 = $"data:image/png;base64,{Convert.ToBase64String(st)}" });
    }
}