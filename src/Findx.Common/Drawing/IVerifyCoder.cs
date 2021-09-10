namespace Findx.Drawing
{
    /// <summary>
    /// 验证码处理器
    /// </summary>
    public interface IVerifyCoder
    {
        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <param name="verifyCodeTyoe">验证码类型</param>
        /// <returns></returns>
        string GetCode(VerifyCodeType verifyCodeTyoe);

        /// <summary>
        /// 创建验证码图片
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        byte[] CreateImage(string text);
    }
}
