using Findx.Swagger;

namespace Findx.UnitTests;

/// <summary>
///     SwaggerOptions 回归测试（修复：Enabled 默认 false，避免生产环境误暴露 API 文档）
/// </summary>
public class SwaggerOptionsTests
{
    [Fact]
    public void Enabled_DefaultsToFalse()
    {
        var options = new SwaggerOptions();

        // D 组行为变更：默认关闭，部署需显式开启
        Assert.False(options.Enabled);
    }

    [Fact]
    public void XmlCommentFiles_NotNullByDefault()
    {
        var options = new SwaggerOptions();
        Assert.NotNull(options.XmlCommentFiles);
    }
}
