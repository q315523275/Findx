using Findx.Extensions.Scheduler.Execution;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Findx.Jobs.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.UnitTests;

/// <summary>
///     执行日志采集器测试：作用域内采集、作用域外空转、并发执行各自隔离、超长日志截断保留尾部。
///     <para>这组语义决定"执行记录里的日志是否可信"：一旦作用域串到别的执行，日志会把两条任务混在一起，
///     排查时会误导判断；作用域外仍采集则会给所有普通请求日志增加开销与噪声。</para>
/// </summary>
public class ExecutionLogCollectorTests
{
    [Fact]
    public void Scope_CapturesLogsInside_AndStopsAfterDispose()
    {
        var collector = new ExecutionLogCollector();

        var scope = collector.Begin("trace-1");
        using (scope)
        {
            scope.Write("开始执行");
            collector.Write(LogLevel.Error, "Findx.Jobs.DemoJob", "业务异常", new InvalidOperationException("boom"));
            Assert.True(collector.IsActive);
        }

        Assert.False(collector.IsActive);

        var text = scope.Render(1000);
        Assert.Contains("开始执行", text);
        Assert.Contains("业务异常", text);
        Assert.Contains("InvalidOperationException", text);

        //  作用域结束后不得再容纳任何日志
        collector.Write(LogLevel.Information, "Findx.Jobs.DemoJob", "作用域外日志", null);
        Assert.DoesNotContain("作用域外日志", scope.Render(1000));
    }

    [Fact]
    public async Task ConcurrentScopes_DoNotMixLogs()
    {
        var collector = new ExecutionLogCollector();

        var first = Task.Run(async () =>
        {
            using var scope = collector.Begin("trace-a");
            await Task.Delay(50);
            collector.Write(LogLevel.Information, "A", "来自 A 的日志", null);
            return scope.Render(1000);
        });

        var second = Task.Run(async () =>
        {
            using var scope = collector.Begin("trace-b");
            await Task.Delay(30);
            collector.Write(LogLevel.Information, "B", "来自 B 的日志", null);
            return scope.Render(1000);
        });

        var results = await Task.WhenAll(first, second);

        Assert.Contains("来自 A 的日志", results[0]);
        Assert.DoesNotContain("来自 B 的日志", results[0]);
        Assert.Contains("来自 B 的日志", results[1]);
        Assert.DoesNotContain("来自 A 的日志", results[1]);
    }

    [Fact]
    public void Render_TruncatesKeepingTail()
    {
        var collector = new ExecutionLogCollector();
        using var scope = collector.Begin("trace-c");

        for (var i = 0; i < 10; i++)
            collector.Write(LogLevel.Information, "C", $"第 {i} 行输出内容", null);

        var text = scope.Render(60);

        Assert.Contains("仅保留尾部", text);
        Assert.Contains("第 9 行", text);
        Assert.DoesNotContain("第 0 行", text);
    }

    [Fact]
    public void RingBuffer_KeepsMostRecentEntriesAndReportsOmitted()
    {
        var collector = new ExecutionLogCollector();
        using var scope = collector.Begin("trace-d");

        for (var i = 0; i < 400; i++)
            collector.Write(LogLevel.Information, "D", $"行 {i}", null);

        var text = scope.Render(0);

        Assert.Contains("省略前 100 条", text);
        Assert.Contains("行 399", text);
        Assert.DoesNotContain("行 99\n", text);
    }

    [Fact]
    public void JobLogRecorder_WritesIntoCurrentScope()
    {
        var collector = new ExecutionLogCollector();
        using var scope = collector.Begin("trace-recorder");
        IJobLogRecorder recorder = scope;

        recorder.Info("处理到第 1 批");
        recorder.Warn("跳过 3 条脏数据");
        recorder.Error("第 2 批失败", new InvalidOperationException("bad"));

        var text = scope.Render(1000);
        Assert.Contains("处理到第 1 批", text);
        Assert.Contains("跳过 3 条脏数据", text);
        Assert.Contains("第 2 批失败", text);
        Assert.Contains("InvalidOperationException", text);
    }

    [Fact]
    public void ChunkSink_ReceivesBatchesOfFifty_AndFinalChunk()
    {
        var chunks = new List<ExecutionLogChunk>();
        var collector = new ExecutionLogCollector();
        var scope = collector.Begin("trace-chunk", 7, chunk => chunks.Add(chunk));

        for (var i = 0; i < 120; i++)
            collector.Write(LogLevel.Information, "BatchJob", $"行 {i}", null);

        //  120 条 -> 前两片各 50 条，剩余 20 条等收尾
        Assert.Equal(2, chunks.Count);
        Assert.All(chunks, c => Assert.Equal(7, c.JobId));
        Assert.Equal(0, chunks[0].Sequence);
        Assert.Equal(1, chunks[1].Sequence);
        Assert.DoesNotContain(chunks, c => c.IsFinal);

        scope.FlushFinal();
        Assert.Equal(3, chunks.Count);
        Assert.True(chunks[2].IsFinal);
        Assert.Contains("行 119", chunks[2].Content);

        //  FlushFinal 幂等：不会重复补发
        scope.FlushFinal();
        Assert.Equal(3, chunks.Count);
        scope.Dispose();
        Assert.Equal(3, chunks.Count);
    }

    [Fact]
    public void Masking_DisabledByDefault_KeepsOriginalText()
    {
        var collector = new ExecutionLogCollector();
        using var scope = collector.Begin("trace-mask-off");
        collector.Write(LogLevel.Information, "MaskJob", "password=Abc123! 手机 13812345678", null);

        var text = scope.Render(0);
        Assert.Contains("Abc123!", text);
        Assert.Contains("13812345678", text);
    }

    [Fact]
    public void Masking_Enabled_MasksSecretsAndPii()
    {
        var options = Options.Create(new SchedulerOptions
        {
            LogMaskingEnabled = true,
            LogMaskPatterns = new List<string> { @"order-\d{4}" }
        });
        var collector = new ExecutionLogCollector(options);
        using var scope = collector.Begin("trace-mask-on");

        collector.Write(LogLevel.Information, "MaskJob",
            "password=Abc123! Authorization: Bearer eyJhbGciOiJIUzI1NiJ9 手机 13812345678 身份证 11010519491231002X 订单 order-2024", null);

        var text = scope.Render(0);
        Assert.DoesNotContain("Abc123!", text);
        Assert.Contains("password=***", text);
        Assert.DoesNotContain("eyJhbGciOiJIUzI1NiJ9", text);
        Assert.DoesNotContain("13812345678", text);
        Assert.DoesNotContain("11010519491231002X", text);
        Assert.DoesNotContain("order-2024", text);
    }
}
