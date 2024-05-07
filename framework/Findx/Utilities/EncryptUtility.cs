using System.Security.Cryptography;
using Findx.Common;
using Findx.Extensions;

namespace Findx.Utilities;

/// <summary>
///     加密辅助操作类
/// </summary>
public static class EncryptUtility
{
    #region SHA256加密

    /// <summary>
    ///     SHA256加密
    /// </summary>
    public static string Sha256(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        var hashValue = SHA256.HashData(bytes);
        return ToHexString(hashValue);
    }

    #endregion

    #region Md5加密

    /// <summary>
    ///     Md5加密，返回16位结果
    /// </summary>
    /// <param name="value">值</param>
    public static string Md5By16(string value)
    {
        return Md5By16(value, Encoding.UTF8);
    }

    /// <summary>
    ///     Md5加密，返回16位结果
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="encoding">字符编码</param>
    public static string Md5By16(string value, Encoding encoding)
    {
        return Md5(value, encoding)?.Sub(8, 16);
    }

    /// <summary>
    ///     Md5加密
    /// </summary>

    private static string Md5(string value, Encoding encoding)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        var inputBytes = encoding.GetBytes(value);
        var hashBytes = MD5.HashData(inputBytes);
        return Convert.ToHexString(hashBytes);
    }

    /// <summary>
    ///     Md5加密，返回32位结果
    /// </summary>
    /// <param name="value">值</param>
    public static string Md5By32(string value)
    {
        return Md5By32(value, Encoding.UTF8);
    }

    /// <summary>
    ///     Md5加密，返回32位结果
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="encoding">字符编码</param>
    public static string Md5By32(string value, Encoding encoding)
    {
        return Md5(value, encoding);
    }

    #endregion

    #region AES加密

    /// <summary>
    ///     AES密钥
    /// </summary>
    private const string AesKey = "MDEyMzQ1Njc4OWFiY2RlZg==";

    /// <summary>
    ///     Aes加密
    /// </summary>
    /// <param name="content"></param>
    /// <param name="key">常规字符串</param>
    /// <returns></returns>
    public static string AesEncrypt(string content, string key = AesKey)
    {
        var toEncryptArray = Encoding.UTF8.GetBytes(content);
        return Convert.ToBase64String(AesEncrypt(toEncryptArray, key));
    }

    /// <summary>
    ///     Aes解密
    /// </summary>
    /// <param name="content"></param>
    /// <param name="key">常规字符串</param>
    /// <returns></returns>
    public static string AesDecrypt(string content, string key = AesKey)
    {
        var toDecryptArray = Convert.FromBase64String(content);
        return Encoding.Default.GetString(AesDecrypt(toDecryptArray, key));
    }

    /// <summary>
    ///     加密字节数组
    /// </summary>
    public static byte[] AesEncrypt(byte[] contentBytes, string key, bool needIv = false)
    {
        contentBytes.ThrowIfNull();
        
        using var aes = Aes.Create();
        
        aes.ThrowIfNull();
        
        aes.Key = CheckKey(key);
        aes.Padding = PaddingMode.PKCS7;
        aes.Mode = CipherMode.ECB;
        
        var ivBytes = Array.Empty<byte>();
        if (needIv)
        {
            aes.Mode = CipherMode.CBC;
            aes.GenerateIV();
            ivBytes = aes.IV;
        }

        using var encryptor = aes.CreateEncryptor();
        
        var encodeBytes = encryptor.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
        aes.Clear();
        
        return needIv ? ivBytes.Concat(encodeBytes).ToArray() : encodeBytes;
    }
    
    /// <summary>
    ///     解密字节数组
    /// </summary>
    public static byte[] AesDecrypt(byte[] encodeBytes, string key, bool needIv = false)
    {
        encodeBytes.ThrowIfNull();

        using var aes = Aes.Create();
        
        aes.ThrowIfNull();
        
        aes.Key = CheckKey(key);
        aes.Padding = PaddingMode.PKCS7;
        aes.Mode = CipherMode.ECB;

        if (needIv)
        {
            aes.Mode = CipherMode.CBC;
            const int ivLength = 16;
            byte[] ivBytes = new byte[ivLength], newEncodeBytes = new byte[encodeBytes.Length - ivLength];
            Array.Copy(encodeBytes, 0, ivBytes, 0, ivLength);
            aes.IV = ivBytes;
            Array.Copy(encodeBytes, ivLength, newEncodeBytes, 0, newEncodeBytes.Length);
            encodeBytes = newEncodeBytes;
        }

        // ReSharper disable once IdentifierTypo
        using var decryptor = aes.CreateDecryptor();
        var decodeBytes = decryptor.TransformFinalBlock(encodeBytes, 0, encodeBytes.Length);
        aes.Clear();
        
        return decodeBytes;
    }

    /// <summary>
    ///     检查密钥，AES加密密钥长度为16位，不足补全多则截断
    /// </summary>
    private static byte[] CheckKey(string key)
    {
        key.ThrowIfNull();
        
        byte[] bytes, keyBytes = new byte[16];

        try
        {
            bytes = Convert.FromBase64String(key);
        }
        catch (FormatException)
        {
            bytes = key.ToBytes();
        }
        
        if (bytes.Length < 16)
        {
            Array.Copy(bytes, 0, keyBytes, 0, bytes.Length);
        }
        else if (bytes.Length > 16)
        {
            Array.Copy(bytes, 0, keyBytes, 0, 16);
        }
        else
        {
            keyBytes = bytes;
        }

        return keyBytes;
    }

    #endregion

    #region Hex16进制工具

    /// <summary>
    ///     ToHexString
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string ToHexString(string text)
    {
        return ToHexString(Encoding.UTF8.GetBytes(text));
    }
    
    /// <summary>
    ///     Hex
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string ToHexString(byte[] text)
    {
        return Convert.ToHexString(text.AsSpan());
    }
    
    /// <summary>
    ///     FromHexString
    /// </summary>
    /// <param name="hexString"></param>
    /// <returns></returns>
    public static byte[] FromHexString(string hexString)
    {
        return Convert.FromHexString(hexString);
    }

    #endregion

    #region HmacSha256加密

    /// <summary>
    ///     HMACSHA256加密
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="key">密钥</param>
    public static string HmacSha256(string value, string key)
    {
        return HmacSha256(value, key, Encoding.UTF8);
    }

    /// <summary>
    ///     HmacSha256加密
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="key">密钥</param>
    /// <param name="encoding">字符编码</param>
    public static string HmacSha256(string value, string key, Encoding encoding)
    {
        if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(key))
            return string.Empty;
        
        var hashValue = HMACSHA256.HashData(encoding.GetBytes(key), encoding.GetBytes(value));
        return ToHexString(hashValue);
    }

    #endregion

    #region Rsa加密

    /// <summary>
    ///     使用指定公钥加密字符串
    /// </summary>
    public static string RsaEncrypt(string source, string publicKey)
    {
        Check.NotNull(source, nameof(source));
        Check.NotNull(publicKey, nameof(publicKey));

        var bytes = Encoding.UTF8.GetBytes(source);
        bytes = RsaEncrypt(bytes, publicKey, RSAEncryptionPadding.Pkcs1);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    ///     使用指定公钥加密字节数组
    /// </summary>
    /// <param name="source"></param>
    /// <param name="publicKey">xml格式公钥</param>
    /// <param name="padding"></param>
    /// <returns></returns>
    public static byte[] RsaEncrypt(byte[] source, string publicKey, RSAEncryptionPadding padding)
    {
        Check.NotNull(source, nameof(source));
        Check.NotNull(publicKey, nameof(publicKey));

        using var rsa = RSA.Create();
        rsa.FromXmlString(publicKey);
        var res = rsa.Encrypt(source, padding);
        rsa.Clear();
        return res;
    }

    /// <summary>
    ///     使用指定私钥解密字符串
    /// </summary>
    /// <param name="source"></param>
    /// <param name="privateKey">xml格式私钥</param>
    public static string RsaDecrypt(string source, string privateKey)
    {
        Check.NotNull(source, nameof(source));
        Check.NotNull(privateKey, nameof(privateKey));

        var bytes = Convert.FromBase64String(source);
        bytes = RsaDecrypt(bytes, privateKey, RSAEncryptionPadding.Pkcs1);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    ///     使用私钥解密字节数组
    /// </summary>
    /// <param name="source"></param>
    /// <param name="privateKey">xml格式私钥</param>
    /// <param name="padding"></param>
    /// <returns></returns>
    public static byte[] RsaDecrypt(byte[] source, string privateKey, RSAEncryptionPadding padding)
    {
        Check.NotNull(source, nameof(source));
        Check.NotNull(privateKey, nameof(privateKey));

        using var rsa = RSA.Create();
        rsa.FromXmlString(privateKey);
        var res = rsa.Decrypt(source, padding);
        rsa.Clear();
        return res;
    }
    
    /// <summary>
    ///     使用指定私钥签名字符串
    /// </summary>
    /// <param name="source">要签名的字符串</param>
    /// <param name="privateKey">xml格式私钥</param>
    /// <returns></returns>
    public static string RsaSignData(string source, string privateKey)
    {
        Check.NotNull(source, nameof(source));
        Check.NotNull(privateKey, nameof(privateKey));

        var bytes = Encoding.UTF8.GetBytes(source);
        var signBytes = RsaSignData(bytes, privateKey, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
        return Convert.ToBase64String(signBytes);
    }

    /// <summary>
    ///     使用指定私钥对明文进行签名，返回明文签名的字节数组
    /// </summary>
    /// <param name="source">要签名的明文字节数组</param>
    /// <param name="privateKey">xml格式私钥</param>
    /// <param name="hashAlgorithmName"></param>
    /// <param name="padding"></param>
    /// <returns>明文签名的字节数组</returns>
    public static byte[] RsaSignData(byte[] source, string privateKey, HashAlgorithmName hashAlgorithmName, RSASignaturePadding padding)
    {
        Check.NotNull(source, nameof(source));
        Check.NotNull(privateKey, nameof(privateKey));

        using var rsa = RSA.Create();
        rsa.FromXmlString(privateKey);
        var res = rsa.SignData(source, hashAlgorithmName, padding);
        rsa.Clear();
        return res;
    }
    
    /// <summary>
    ///     使用指定公钥验证解密得到的明文是否符合签名
    /// </summary>
    /// <param name="source">解密得到的明文</param>
    /// <param name="signData">明文签名的BASE64字符串</param>
    /// <param name="publicKey">xml格式公钥</param>
    /// <returns>验证是否通过</returns>
    public static bool RsaVerifyData(string source, string signData, string publicKey)
    {
        Check.NotNull(source, nameof(source));
        Check.NotNull(signData, nameof(signData));

        var sourceBytes = Encoding.UTF8.GetBytes(source);
        var signBytes = Convert.FromBase64String(signData);
        return RsaVerifyData(sourceBytes, signBytes, publicKey, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
    }

    /// <summary>
    ///     使用指定公钥验证解密得到的明文是否符合签名
    /// </summary>
    /// <param name="source">解密的明文字节数组</param>
    /// <param name="signData">明文签名字节数组</param>
    /// <param name="publicKey">xml格式公钥</param>
    /// <param name="hashAlgorithmName"></param>
    /// <param name="padding"></param>
    /// <returns>验证是否通过</returns>
    public static bool RsaVerifyData(byte[] source, byte[] signData, string publicKey, HashAlgorithmName hashAlgorithmName, RSASignaturePadding padding)
    {
        Check.NotNull(source, nameof(source));
        Check.NotNull(publicKey, nameof(publicKey));

        using var rsa = RSA.Create();
        rsa.FromXmlString(publicKey);
        var res = rsa.VerifyData(source, signData, hashAlgorithmName, padding);
        rsa.Clear();
        return res;
    }

    #endregion
}