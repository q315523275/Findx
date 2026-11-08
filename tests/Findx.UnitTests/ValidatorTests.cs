using System.ComponentModel.DataAnnotations;
using Findx.Validations.Validators;

namespace Findx.UnitTests;

/// <summary>
///     校验器回归测试（修复：IdCard 校验位 ISO 7064 MOD 11-2；UrlPattern 嵌套量词 ReDoS）
/// </summary>
public class ValidatorTests
{
    [Theory]
    [InlineData("11010519491231002X")]  // 合法 18 位，校验位 X
    [InlineData("320311770706001")]     // 合法 15 位（19xx 出生）
    [InlineData("")]                    // 空放行
    [InlineData(null)]
    public void IdCard_Valid_ReturnsSuccess(object value)
    {
        var attr = new IdCardAttribute();
        Assert.True(attr.IsValid(value));
    }

    [Theory]
    [InlineData("110105194912310021")]  // 校验位错误
    [InlineData("110105194913310025")]  // 非法日期（13 月）
    [InlineData("1234567890123456789")] // 长度 19
    public void IdCard_Invalid_ReturnsFalse(object value)
    {
        var attr = new IdCardAttribute();
        Assert.False(attr.IsValid(value));
    }

    [Fact]
    public void UrlPattern_NormalUrl_Matches()
    {
        Assert.Matches(ValidatePattern.UrlPattern, "https://www.example.com/path?q=1&a=b");
        Assert.Matches(ValidatePattern.UrlPattern, "http://example.com");
    }

    [Fact]
    public void UrlPattern_NoScheme_Rejected()
    {
        Assert.DoesNotMatch(ValidatePattern.UrlPattern, "example.com/path");
    }

    [Fact]
    public void UrlPattern_HostileInput_ReturnsQuickly()
    {
        // 段式攻击串：大量 "a." 片段 + 非法结尾，修复前嵌套量词 ([w-]+(\.[w-]+)*/)* 指数回溯；修复后线性快速返回
        var hostile = "http://" + string.Concat(Enumerable.Repeat("a.", 3000)) + "!";
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var matched = System.Text.RegularExpressions.Regex.IsMatch(hostile, ValidatePattern.UrlPattern);
        sw.Stop();

        Assert.False(matched);
        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(2), $"ReDoS 防御失败：耗时 {sw.Elapsed}");
    }

    [Fact]
    public void CollectionSize_WithinRange_Success()
    {
        var attr = new CollectionSizeAttribute { Min = 1, Max = 5 };
        Assert.True(attr.IsValid(new List<int> { 1, 2, 3 }));
        Assert.True(attr.IsValid(null));
        Assert.True(attr.IsValid("字符串放行"));
    }

    [Fact]
    public void CollectionSize_ExceedMax_Fails()
    {
        var attr = new CollectionSizeAttribute { Min = 1, Max = 2 };
        Assert.False(attr.IsValid(new List<int> { 1, 2, 3 }));
    }

    [Fact]
    public void CollectionSize_BelowMin_Fails()
    {
        var attr = new CollectionSizeAttribute { Min = 3 };
        Assert.False(attr.IsValid(new List<int> { 1, 2 }));
    }
}
