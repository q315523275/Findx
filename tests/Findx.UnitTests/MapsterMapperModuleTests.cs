using Findx.Mapster;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.UnitTests;

/// <summary>
///     Mapster 三元映射回归测试（修复：IsNullOrWhiteSpace ? null : Parse 写反导致非空串映射为 null / 空串 Parse 崩溃）
/// </summary>
public class MapsterMapperModuleTests
{
    private static void ActivateModule()
    {
        var services = new ServiceCollection();
        new MapsterMapperModule().ConfigureServices(services);
        var provider = services.BuildServiceProvider();
        new MapsterMapperModule().UseModule(provider);
    }

    [Fact]
    public void StringToNullableDateTime_ValidDate_Parses()
    {
        ActivateModule();

        var parsed = "2026-11-03".Adapt<DateTime?>();

        // 修复前三元写反：非空串映射为 null
        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(2026, 11, 3), parsed.Value);
    }

    [Fact]
    public void StringToNullableDateTime_EmptyString_ReturnsNull()
    {
        ActivateModule();

        // 修复前空串走 Parse → FormatException
        Assert.Null("".Adapt<DateTime?>());
    }

    [Fact]
    public void StringToNullableDateTime_Whitespace_ReturnsNull()
    {
        ActivateModule();

        Assert.Null("   ".Adapt<DateTime?>());
    }
}
