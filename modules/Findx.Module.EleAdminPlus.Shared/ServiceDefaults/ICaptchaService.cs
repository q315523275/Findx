using Findx.Data;

namespace Findx.Module.EleAdminPlus.Shared.ServiceDefaults;

/// <summary>
/// 验证码服务接口
/// </summary>
public interface ICaptchaService
{
    /// <summary>
    /// 生成验证码
    /// </summary>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <param name="length">长度</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>验证码结果</returns>
    Task<CommonResult> GenerateCaptchaAsync(int width, int height, int length, CancellationToken cancellationToken = default);
}