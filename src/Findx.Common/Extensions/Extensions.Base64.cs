namespace Findx.Extensions;

/// <summary>
///     系统扩展 - Base64转换
/// </summary>
public partial class Extensions
{
    /// <summary>
    ///     将字符串转为base64字符串
    /// </summary>
    /// <param name="input">要转化的字符串</param>
    /// <param name="encode">编码格式默认Utf8</param>
    /// <returns>base64字符串</returns>
    public static string ToBase64(this string input, Encoding encode = null)
    {
        Check.NotNull(input, nameof(input));

        if (encode == null) encode = Encoding.UTF8;
        ;
        return Convert.ToBase64String(encode.GetBytes(input));
    }

    /// <summary>
    ///     将字节数组转为base64字符串
    /// </summary>
    /// <param name="input">要转化的字节数组</param>
    /// <returns>base64字符串</returns>
    public static string ToBase64(this byte[] input)
    {
        Check.NotNull(input, nameof(input));

        return Convert.ToBase64String(input);
    }

    /// <summary>
    ///     base64字符解密
    /// </summary>
    /// <param name="input">需要解密的字符信息</param>
    /// <param name="encode">编码格式默认Utf8</param>
    /// <returns>解密后的字符信息</returns>
    public static string DecodeBase64(this string input, Encoding encode)
    {
        Check.NotNull(input, nameof(input));

        if (encode == null) encode = Encoding.UTF8;
        ;
        return encode.GetString(Convert.FromBase64String(input));
    }

    /// <summary>
    ///     base64字符解密
    /// </summary>
    /// <param name="input">需要解密的字符信息</param>
    /// <returns>解密后的字节数组</returns>
    public static byte[] DecodeBase64(this string input)
    {
        Check.NotNull(input, nameof(input));

        return Convert.FromBase64String(input);
        ;
    }
}