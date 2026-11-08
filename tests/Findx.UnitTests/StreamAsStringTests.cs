using System.Text;
using Findx.Extensions;

namespace Findx.UnitTests;

/// <summary>
///     Stream 扩展回归测试（补充：AsStringAsync 对不可 Seek 流不再抛异常；修复前无条件 Seek 抛 NotSupportedException）
/// </summary>
public class StreamAsStringTests
{
    private sealed class NonSeekableStream : MemoryStream
    {
        public NonSeekableStream(byte[] data) : base(data) { }
        public override bool CanSeek => false;
    }

    [Fact]
    public async Task AsStringAsync_NonSeekableStream_Works()
    {
        await using var stream = new NonSeekableStream("findx-test"u8.ToArray());

        // 修复前：AsStringInternalAsync 无条件 Seek(0) → NotSupportedException
        var result = await stream.AsStringAsync();

        Assert.Equal("findx-test", result);
    }

    [Fact]
    public async Task AsStringAsync_Utf8Content_Works()
    {
        await using var stream = new MemoryStream("中文内容"u8.ToArray());

        var result = await stream.AsStringAsync(Encoding.UTF8);
        Assert.Equal("中文内容", result);
    }
}
