using System.Threading.Tasks;

namespace Findx.Imaging;

/// <summary>
///     验证码处理器
/// </summary>
public interface IVerifyCoder
{
    /// <summary>
    /// 设置验证码字体
    /// </summary>
    /// <param name="fontFamilyPath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SetFontAsync(string fontFamilyPath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 设置验证码字体
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SetFontAsync(Stream stream, CancellationToken cancellationToken = default);
    
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
    /// <param name="fontSize">字体大小:默认50</param>
    /// <returns></returns>
    Task<byte[]> CreateImageAsync(string text, int width = 120, int height = 50, int fontSize = 32);
    
    /// <summary>
    ///     创建验证码图片
    /// </summary>
    /// <param name="text"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="fontSize">字体大小:默认50</param>
    /// <returns></returns>
    Task<Stream> CreateImageStreamAsync(string text, int width = 120, int height = 50, int fontSize = 32);
}