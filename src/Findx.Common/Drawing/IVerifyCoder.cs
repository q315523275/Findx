using System.Threading.Tasks;

namespace Findx.Drawing;

/// <summary>
///     验证码处理器
/// </summary>
public interface IVerifyCoder
{
    /// <summary>
    ///     获取验证码
    /// </summary>
    /// <param name="length">验证码长度</param>
    /// <param name="verifyCodeType">验证码类型</param>
    /// <returns></returns>
    string GetCode(int length, VerifyCodeType verifyCodeType);

    /// <summary>
    ///     创建验证码图片
    /// </summary>
    /// <param name="text"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    Task<byte[]> CreateImageAsync(string text, int width = 120, int height = 50);
}