using Findx.Utilities;

namespace Findx.UnitTests;

/// <summary>
///     EncryptUtility 回归测试（修复：AesDecrypt Encoding.Default → UTF-8，中文内容解密不再乱码）
/// </summary>
public class EncryptUtilityTests
{
    [Theory]
    [InlineData("hello world")]
    [InlineData("中文内容 ✅ 特殊符号 ①②③")]
    [InlineData("")]
    public void Aes_RoundTrip_PreservesContent(string plain)
    {
        var encrypted = EncryptUtility.AesEncrypt(plain);
        var decrypted = EncryptUtility.AesDecrypt(encrypted);

        Assert.Equal(plain, decrypted);
    }

    [Fact]
    public void Aes_RepeatedEncrypt_DecryptsBack()
    {
        // 当前实现为已知遗留的固定密钥/ECB（记录不修项），同一明文两次密文相同属预期；验证两次均可解密还原
        var a = EncryptUtility.AesEncrypt("abc");
        var b = EncryptUtility.AesEncrypt("abc");

        Assert.Equal(a, b);
        Assert.Equal("abc", EncryptUtility.AesDecrypt(a));
        Assert.Equal("abc", EncryptUtility.AesDecrypt(b));
    }
}
