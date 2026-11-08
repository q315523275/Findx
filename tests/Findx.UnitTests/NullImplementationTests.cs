using Findx.Net.IP;
using Findx.Tracing;
using Microsoft.Extensions.Logging.Abstractions;

namespace Findx.UnitTests;

/// <summary>
///     占位实现回归测试（修复：NullIpGeolocation 由抛 NotImplementedException 改空实现；CorrelationIdOptions.Value 恒 null 一行修）
/// </summary>
public class NullImplementationTests
{
    [Fact]
    public void NullIpGeolocation_ReturnsNull_NoThrow()
    {
        var geo = new NullIpGeolocation(NullLogger<NullIpGeolocation>.Instance);

        // 修复前：GetLocationTextByIp 抛 NotImplementedException
        Assert.Null(geo.GetLocationTextByIp("1.2.3.4"));
        Assert.Null(geo.GetLocationByIp("1.2.3.4"));
        Assert.Empty(geo.BatchGetLocationText(["1.2.3.4"]));
        Assert.Empty(geo.BatchGetLocation(["1.2.3.4"]));
    }

    [Fact]
    public void CorrelationIdOptions_Value_ReturnsSelf()
    {
        var options = new CorrelationIdOptions();

        // 修复前：Value 恒返回 null → 使用方 NRE
        Assert.Same(options, options.Value);
        Assert.Equal("X-Correlation-Id", options.HttpHeaderName);
    }
}
