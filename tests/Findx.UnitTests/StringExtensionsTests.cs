using Findx.Extensions;
using Xunit;

namespace Findx.UnitTests;

/// <summary>
///     字符串扩展回归测试
///     <para>覆盖正则静态化与「正则改字符循环」改造后的行为等价性：IsMatchNumber 两个重载、Unicode 编解码往返、身份证校验。</para>
/// </summary>
public class StringExtensionsTests
{
    [Theory]
    [InlineData("12345", 5, true)]
    [InlineData("1234", 5, false)]
    [InlineData("123456", 5, false)]
    [InlineData("12a45", 5, false)]
    [InlineData(" 1234", 5, false)]
    [InlineData("", 0, true)]
    public void IsMatchNumber_WithLength_MatchesDigitsOnly(string value, int length, bool expected)
    {
        // 原实现每次按长度拼 pattern 再 new Regex，改造后为字符区间判断，行为必须一致
        Assert.Equal(expected, value.IsMatchNumber(length));
    }

    [Theory]
    [InlineData("abc1", true)]
    [InlineData("1abc", true)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    [InlineData("１２３", true)] // 全角数字属于 Unicode Nd 类别，与正则 \d 语义一致
    public void IsMatchNumber_ContainsDigit_SameAsRegexD(string value, bool expected)
    {
        Assert.Equal(expected, value.IsMatchNumber());
    }

    [Fact]
    public void ToUnicodeString_And_FromUnicodeString_RoundTrip()
    {
        var source = "中文abc";

        var encoded = source.ToUnicodeString();
        Assert.Equal("\\u4e2d\\u6587abc", encoded);

        var decoded = encoded.FromUnicodeString();
        Assert.Equal(source, decoded);
    }

    [Fact]
    public void ToUnicodeString_PureAscii_Unchanged()
    {
        Assert.Equal("findx-2026", "findx-2026".ToUnicodeString());
    }

    [Fact]
    public void IsIdentityCardId_Valid18Digits_ReturnsTrue()
    {
        // 11010519491231002X 为校验位正确的标准样例
        Assert.True("11010519491231002X".IsIdentityCardId());
    }

    [Fact]
    public void IsIdentityCardId_WrongCheckCode_ReturnsFalse()
    {
        Assert.False("110105194912310021".IsIdentityCardId());
    }

    [Fact]
    public void IsIdentityCardId_InvalidLength_ReturnsFalse()
    {
        Assert.False("11010519491231".IsIdentityCardId());
    }

    [Fact]
    public void IsIdentityCardId_15Digits_ValidDate_ReturnsTrue()
    {
        // 修复点：15 位结构正则结尾原为 (\d{3})_（要求末尾下划线），导致 15 位身份证永远校验失败；改为 $ 后正常匹配
        Assert.True("110105491231002".IsIdentityCardId());
    }

    [Fact]
    public void IsIdentityCardId_15Digits_InvalidDate_ReturnsFalse()
    {
        // 结构合法但日期非法（13 月），由 DateTime.TryParse 兜底拒绝
        Assert.False("110105491331002".IsIdentityCardId());
    }

    [Fact]
    public void IsIdentityCardId_15Digits_WithNonDigitChar_ReturnsFalse()
    {
        Assert.False("11010549123100X".IsIdentityCardId());
    }

    [Fact]
    public void IsMobileNumber_ValidAndInvalid()
    {
        Assert.True("13800138000".IsMobileNumber(true));
        Assert.False("1380013800".IsMobileNumber(true));
        Assert.False("abcdefghijk".IsMobileNumber());
    }
}
