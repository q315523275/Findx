using Findx.Extensions;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
namespace Findx.Utils
{
    /// <summary>
    /// 加密辅助操作类
    /// </summary>
    public static class Encrypt
    {
        #region Md5加密

        /// <summary>
        /// Md5加密，返回16位结果
        /// </summary>
        /// <param name="value">值</param>
        public static string Md5By16(string value)
        {
            return Md5By16(value, Encoding.UTF8);
        }

        /// <summary>
        /// Md5加密，返回16位结果
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="encoding">字符编码</param>
        public static string Md5By16(string value, Encoding encoding)
        {
            return Md5(value, encoding, 4, 8);
        }

        /// <summary>
        /// Md5加密
        /// </summary>
        private static string Md5(string value, Encoding encoding, int? startIndex, int? length)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            var md5 = new MD5CryptoServiceProvider();
            string result;
            try
            {
                var hash = md5.ComputeHash(encoding.GetBytes(value));
                result = startIndex == null ? BitConverter.ToString(hash) : BitConverter.ToString(hash, startIndex.SafeValue(), length.SafeValue());
            }
            finally
            {
                md5.Clear();
            }
            return result.Replace("-", "").ToLower();
        }

        /// <summary>
        /// Md5加密，返回32位结果
        /// </summary>
        /// <param name="value">值</param>
        public static string Md5By32(string value)
        {
            return Md5By32(value, Encoding.UTF8);
        }

        /// <summary>
        /// Md5加密，返回32位结果
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="encoding">字符编码</param>
        public static string Md5By32(string value, Encoding encoding)
        {
            return Md5(value, encoding, null, null);
        }

        #endregion

        #region DES加密

        /// <summary>
        /// DES密钥,24位字符串
        /// </summary>
        public static string DesKey = "#s^un2ye21fcv%|f0XpR,+vh";

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="value">待加密的值</param>
        public static string DesEncrypt(object value)
        {
            return DesEncrypt(value, DesKey);
        }

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="value">待加密的值</param>
        /// <param name="key">密钥,24位</param>
        public static string DesEncrypt(object value, string key)
        {
            return DesEncrypt(value, key, Encoding.UTF8);
        }

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="value">待加密的值</param>
        /// <param name="key">密钥,24位</param>
        /// <param name="encoding">编码</param>
        public static string DesEncrypt(object value, string key, Encoding encoding)
        {
            string text = value.SafeString();
            if (ValidateDes(text, key) == false)
                return string.Empty;
            using (var transform = CreateDesProvider(key).CreateEncryptor())
            {
                return GetEncryptResult(text, encoding, transform);
            }
        }

        /// <summary>
        /// 验证Des加密参数
        /// </summary>
        private static bool ValidateDes(string text, string key)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(key))
                return false;
            return key.Length == 24;
        }

        /// <summary>
        /// 创建Des加密服务提供程序
        /// </summary>
        private static TripleDESCryptoServiceProvider CreateDesProvider(string key)
        {
            return new TripleDESCryptoServiceProvider { Key = Encoding.ASCII.GetBytes(key), Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 };
        }

        /// <summary>
        /// 获取加密结果
        /// </summary>
        private static string GetEncryptResult(string value, Encoding encoding, ICryptoTransform transform)
        {
            var bytes = encoding.GetBytes(value);
            var result = transform.TransformFinalBlock(bytes, 0, bytes.Length);
            return System.Convert.ToBase64String(result);
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="value">加密后的值</param>
        public static string DesDecrypt(object value)
        {
            return DesDecrypt(value, DesKey);
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="value">加密后的值</param>
        /// <param name="key">密钥,24位</param>
        public static string DesDecrypt(object value, string key)
        {
            return DesDecrypt(value, key, Encoding.UTF8);
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="value">加密后的值</param>
        /// <param name="key">密钥,24位</param>
        /// <param name="encoding">编码</param>
        public static string DesDecrypt(object value, string key, Encoding encoding)
        {
            string text = value.SafeString();
            if (!ValidateDes(text, key))
                return string.Empty;
            using (var transform = CreateDesProvider(key).CreateDecryptor())
            {
                return GetDecryptResult(text, encoding, transform);
            }
        }

        /// <summary>
        /// 获取解密结果
        /// </summary>
        private static string GetDecryptResult(string value, Encoding encoding, ICryptoTransform transform)
        {
            var bytes = System.Convert.FromBase64String(value);
            var result = transform.TransformFinalBlock(bytes, 0, bytes.Length);
            return encoding.GetString(result);
        }

        #endregion

        #region AES加密
        /// <summary>
        /// AES密钥
        /// </summary>
        private static string _aeskey = "0123456789abcdef";
        /// <summary>
        /// Aes加密
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string AesEncrypt(string content)
        {
            return AesEncrypt(content, _aeskey);
        }
        /// <summary>
        /// Aes加密
        /// </summary>
        /// <param name="content"></param>
        /// <param name="key">常规字符串</param>
        /// <returns></returns>
        public static string AesEncrypt(string content, string key)
        {
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(content);
            var des = CreateSymmetricAlgorithm(key);

            using (var cTransform = des.CreateEncryptor())
            {
                byte[] resultArray = GetTransformFinalBlock(cTransform, toEncryptArray);
                return Convert.ToBase64String(resultArray);
            }
        }
        /// <summary>
        /// Aes解密
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string AesDecrypt(string content)
        {
            return AesDecrypt(content, _aeskey);
        }
        /// <summary>
        /// Aes解密
        /// </summary>
        /// <param name="content"></param>
        /// <param name="key">常规字符串</param>
        /// <returns></returns>
        public static string AesDecrypt(string content, string key)
        {
            byte[] toEncryptArray = System.Convert.FromBase64String(content);
            var des = CreateSymmetricAlgorithm(key);

            using (var cTransform = des.CreateDecryptor())
            {
                byte[] resultArray = GetTransformFinalBlock(cTransform, toEncryptArray);
                return Encoding.Default.GetString(resultArray);
            }
        }
        /// <summary>
        /// 创建SymmetricAlgorithm
        /// </summary>
        /// <param name="aesKey"></param>
        /// <returns></returns>
        private static SymmetricAlgorithm CreateSymmetricAlgorithm(string aesKey)
        {
            byte[] keyArray = Encoding.UTF8.GetBytes(aesKey);
            SymmetricAlgorithm des = Aes.Create();
            des.Key = keyArray;
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;
            return des;
        }
        private static byte[] GetTransformFinalBlock(ICryptoTransform cTransform, byte[] dateArray)
        {
            return cTransform.TransformFinalBlock(dateArray, 0, dateArray.Length);
        }
        #endregion

        #region SHA256加密
        /// <summary>
        /// SHA256加密
        /// </summary>
        public static string SHA256(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            SHA256Managed managed = new SHA256Managed();
            return ToHexString(managed.ComputeHash(bytes));
        }
        /// <summary>
        /// Hex
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToHexString(byte[] text)
        {
            StringBuilder ret = new StringBuilder();
            foreach (byte b in text)
            {
                ret.AppendFormat("{0:x2}", b);
            }
            return ret.ToString();
        }
        /// <summary>
        /// ToHexString
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToHexString(string text)
        {
            return ToHexString(Encoding.UTF8.GetBytes(text));
        }
        /// <summary>
        /// FromHexString
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] FromHexString(string hexString)
        {
            byte[] data = null;
            try
            {
                if (!string.IsNullOrEmpty(hexString))
                {
                    int length = hexString.Length / 2;
                    data = new byte[length];
                    for (int i = 0; i < length; i++)
                    {
                        data[i] = Convert.ToByte(hexString.Substring(2 * i, 2), 16);
                    }
                }
            }
            catch
            {

            }
            return data;
        }
        #endregion

        #region HmacSha256加密

        /// <summary>
        /// HMACSHA256加密
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="key">密钥</param>
        public static string HmacSha256(string value, string key)
        {
            return HmacSha256(value, key, Encoding.UTF8);
        }

        /// <summary>
        /// HMACSHA256加密
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="key">密钥</param>
        /// <param name="encoding">字符编码</param>
        public static string HmacSha256(string value, string key, Encoding encoding)
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(key))
                return string.Empty;
            var sha256 = new HMACSHA256(encoding.GetBytes(key));
            var hash = sha256.ComputeHash(encoding.GetBytes(value));
            return string.Join("", hash.ToList().Select(t => t.ToString("x2")).ToArray());
        }

        #endregion

        #region Rsa加密

        /// <summary>
        /// 使用指定公钥加密字节数组
        /// </summary>
        /// <param name="source"></param>
        /// <param name="publicKey">xml格式公钥</param>
        /// <returns></returns>
        public static byte[] RsaEncrypt(byte[] source, string publicKey)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(publicKey, nameof(publicKey));

            RSA rsa = RSA.Create();
            rsa.FromXmlStringEx(publicKey);
            return rsa.Encrypt(source, RSAEncryptionPadding.Pkcs1);
        }

        /// <summary>
        /// 使用私钥解密字节数组
        /// </summary>
        /// <param name="source"></param>
        /// <param name="privateKey">xml格式私钥</param>
        /// <returns></returns>
        public static byte[] RsaDecrypt(byte[] source, string privateKey)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(privateKey, nameof(privateKey));

            RSA rsa = RSA.Create();
            rsa.FromXmlStringEx(privateKey);
            return rsa.Decrypt(source, RSAEncryptionPadding.Pkcs1);
        }

        /// <summary>
        /// 使用指定私钥对明文进行签名，返回明文签名的字节数组
        /// </summary>
        /// <param name="source">要签名的明文字节数组</param>
        /// <param name="privateKey">xml格式私钥</param>
        /// <returns>明文签名的字节数组</returns>
        public static byte[] RsaSignData(byte[] source, string privateKey)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(privateKey, nameof(privateKey));

            RSA rsa = RSA.Create();
            rsa.FromXmlStringEx(privateKey);
            return rsa.SignData(source, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
        }

        /// <summary>
        /// 使用指定公钥验证解密得到的明文是否符合签名
        /// </summary>
        /// <param name="source">解密的明文字节数组</param>
        /// <param name="signData">明文签名字节数组</param>
        /// <param name="publicKey">xml格式公钥</param>
        /// <returns>验证是否通过</returns>
        public static bool RsaVerifyData(byte[] source, byte[] signData, string publicKey)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(publicKey, nameof(publicKey));

            RSA rsa = RSA.Create();
            rsa.FromXmlStringEx(publicKey);
            return rsa.VerifyData(source, signData, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
        }

        /// <summary>
        /// 使用指定公钥加密字符串
        /// </summary>
        public static string RsaEncrypt(string source, string publicKey)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(publicKey, nameof(publicKey));

            byte[] bytes = Encoding.UTF8.GetBytes(source);
            bytes = RsaEncrypt(bytes, publicKey);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 使用指定私钥解密字符串
        /// </summary>
        public static string RsaDecrypt(string source, string privateKey)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(privateKey, nameof(privateKey));

            byte[] bytes = Convert.FromBase64String(source);
            bytes = RsaDecrypt(bytes, privateKey);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 使用指定私钥签名字符串
        /// </summary>
        /// <param name="source">要签名的字符串</param>
        /// <param name="privateKey">私钥</param>
        /// <returns></returns>
        public static string RsaSignData(string source, string privateKey)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(privateKey, nameof(privateKey));

            byte[] bytes = Encoding.UTF8.GetBytes(source);
            byte[] signBytes = RsaSignData(bytes, privateKey);
            return Convert.ToBase64String(signBytes);
        }

        /// <summary>
        /// 使用指定公钥验证解密得到的明文是否符合签名
        /// </summary>
        /// <param name="source">解密得到的明文</param>
        /// <param name="signData">明文签名的BASE64字符串</param>
        /// <param name="publicKey">公钥</param>
        /// <returns>验证是否通过</returns>
        public static bool RsaVerifyData(string source, string signData, string publicKey)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(signData, nameof(signData));

            byte[] sourceBytes = Encoding.UTF8.GetBytes(source);
            byte[] signBytes = Convert.FromBase64String(signData);
            return RsaVerifyData(sourceBytes, signBytes, publicKey);
        }

        #endregion
    }
}
