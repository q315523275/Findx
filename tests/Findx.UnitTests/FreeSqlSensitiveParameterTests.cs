using System.Reflection;
using Findx.FreeSql;
using Xunit;

namespace Findx.UnitTests;

/// <summary>
///     敏感参数判定回归测试（FreeSqlModule 审计脱敏）
///     <para>IsSensitiveParameter 为私有静态方法，此处通过反射验证：关键字忽略大小写匹配、
///     空白输入判非敏感、缓存命中路径与首次解析结果一致。</para>
/// </summary>
public class FreeSqlSensitiveParameterTests
{
    private static readonly MethodInfo IsSensitiveMethod =
        typeof(FreeSqlModule).GetMethod("IsSensitiveParameter", BindingFlags.NonPublic | BindingFlags.Static);

    private static bool IsSensitiveParameter(string name)
    {
        Assert.NotNull(IsSensitiveMethod);
        return (bool)IsSensitiveMethod.Invoke(null, new object[] { name });
    }

    [Theory]
    [InlineData("Password", true)]
    [InlineData("user_password", true)]
    [InlineData("ACCESS_TOKEN", true)]
    [InlineData("IdCard", true)]
    [InlineData("id_no", true)]
    [InlineData("mobile", true)]
    [InlineData("userName", false)]
    [InlineData("created_at", false)]
    [InlineData("order_id", false)]
    public void IsSensitiveParameter_MatchesKeywordsIgnoreCase(string name, bool expected)
    {
        Assert.Equal(expected, IsSensitiveParameter(name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void IsSensitiveParameter_BlankInput_ReturnsFalse(string name)
    {
        Assert.False(IsSensitiveParameter(name));
    }

    [Fact]
    public void IsSensitiveParameter_RepeatedCalls_ReturnConsistentResult()
    {
        // 首次走关键字遍历并写入缓存，后续走缓存快路径，结果必须一致
        var first = IsSensitiveParameter("bank_card_no");
        var second = IsSensitiveParameter("bank_card_no");
        var third = IsSensitiveParameter("bank_card_no");

        Assert.True(first);
        Assert.Equal(first, second);
        Assert.Equal(first, third);
    }
}
