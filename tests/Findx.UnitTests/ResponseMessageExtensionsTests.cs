using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Findx.WebSocketCore;
using Findx.WebSocketCore.Extensions;
using Xunit;

namespace Findx.UnitTests;

/// <summary>
///     ResponseMessage 文本读取回归测试
///     <para>覆盖 AsTextAsync 的编码/BOM/重复读取语义：UTF-8 + detectBOM=true（BOM 不残留）、leaveOpen=true、
///     流已读空后自动回到起点，保证同一消息可重复读取。</para>
/// </summary>
public class ResponseMessageExtensionsTests
{
    [Fact]
    public async Task AsTextAsync_StreamMessage_ReturnsText()
    {
        using var stream = new MemoryStream("hello"u8.ToArray());
        var message = ResponseMessage.BinaryStreamMessage(stream);

        Assert.Equal("hello", await message.AsTextAsync());
    }

    [Fact]
    public async Task AsTextAsync_StreamWithUtf8Bom_BomIsNotLeaked()
    {
        // BOM 前缀若未被识别跳过，会以 U+FEFF 残留在首字符，导致 JSON 反序列化失败
        var payload = new byte[] { 0xEF, 0xBB, 0xBF }.Concat("{\"a\":1}"u8.ToArray()).ToArray();
        using var stream = new MemoryStream(payload);
        var message = ResponseMessage.BinaryStreamMessage(stream);

        var text = await message.AsTextAsync();

        Assert.Equal("{\"a\":1}", text);
        Assert.DoesNotContain('\uFEFF', text);
    }

    [Fact]
    public async Task AsTextAsync_CalledTwice_ReturnsFullTextBothTimes()
    {
        using var stream = new MemoryStream("repeat"u8.ToArray());
        var message = ResponseMessage.BinaryStreamMessage(stream);

        Assert.Equal("repeat", await message.AsTextAsync());
        // 第二次调用时流已处于 EOF，方法应自动回到起点而不是返回空串
        Assert.Equal("repeat", await message.AsTextAsync());
    }

    [Fact]
    public async Task AsTextAsync_ChineseContent_NoMojibake()
    {
        using var stream = new MemoryStream("中文内容"u8.ToArray());
        var message = ResponseMessage.BinaryStreamMessage(stream);

        Assert.Equal("中文内容", await message.AsTextAsync());
    }

    [Fact]
    public async Task AsTextAsync_MemoryMessage_ReturnsText()
    {
        var message = ResponseMessage.BinaryMessage("memory-内容"u8.ToArray());

        Assert.Equal("memory-内容", await message.AsTextAsync());
    }

    [Fact]
    public async Task AsTextAsync_NullStream_ReturnsEmpty()
    {
        var message = ResponseMessage.BinaryStreamMessage(null);

        Assert.Equal(string.Empty, await message.AsTextAsync());
    }
}
