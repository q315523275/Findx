using Findx.Exceptions;

namespace Findx.UnitTests;

/// <summary>
///     FindxException 回归测试（修复：Message 不再拼接 inner.Message，防止内部敏感信息泄漏）
/// </summary>
public class FindxExceptionTests
{
    [Fact]
    public void Message_WithInner_DoesNotLeakInnerMessage()
    {
        var inner = new InvalidOperationException("内部数据库连接字符串 db:secret");
        var ex = new FindxException("CODE", "用户可读的外层错误", inner);

        // 修复前：base(message + inner.Message) 拼接，内部细节进入用户可见 Message
        Assert.Equal("用户可读的外层错误", ex.Message);
        Assert.DoesNotContain("db:secret", ex.Message);
        Assert.Same(inner, ex.InnerException);
        Assert.Equal("CODE", ex.ErrorCode);
    }

    [Fact]
    public void Message_NoInner_UsesMessage()
    {
        var ex = new FindxException("CODE", "普通错误");
        Assert.Equal("普通错误", ex.Message);
    }
}
