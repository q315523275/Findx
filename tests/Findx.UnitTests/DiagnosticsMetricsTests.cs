using System.Diagnostics.Metrics;
using Findx.Metrics;

namespace Findx.UnitTests;

/// <summary>
///     DiagnosticsMetrics 回归测试（修复：GetOrAdd 值参数重载每次新建同名仪器 → 工厂委托重载，同名仅 1 个仪器）
/// </summary>
public class DiagnosticsMetricsTests
{
    [Fact]
    public void Counter_SameNameManyTimes_CreatesSingleInstrument()
    {
        using var metrics = new DiagnosticsMetrics();
        using var listener = new MeterListener();
        var published = 0;

        listener.InstrumentPublished = (instrument, _) =>
        {
            if (instrument.Name == "test.counter") Interlocked.Increment(ref published);
        };
        listener.Start();

        for (var i = 0; i < 1000; i++) metrics.Counter("test.counter", 1);

        // 修复前每次调用都向 Meter 注册同名仪器；修复后工厂委托仅创建 1 个
        Assert.Equal(1, published);
    }

    [Fact]
    public void Counter_Gauge_Timer_NoThrow()
    {
        using var metrics = new DiagnosticsMetrics();

        metrics.Counter("ops", 1);
        metrics.Gauge("load", 0.5);
        metrics.Timer("latency", 100);
    }
}
