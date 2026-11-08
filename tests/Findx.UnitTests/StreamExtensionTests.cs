using Findx.Extensions;

namespace Findx.UnitTests;

/// <summary>
///     Stream 扩展回归测试（修复：单次 ReadAsync 短读截断、ContentsEqual 只比首块即 true）
/// </summary>
public class StreamExtensionTests
{
    /// <summary>每次 ReadAsync 只返回少量字节的流，模拟短读场景</summary>
    private sealed class TrickleStream : Stream
    {
        private readonly byte[] _data;
        private int _pos;
        private readonly int _chunk;

        public TrickleStream(byte[] data, int chunk)
        {
            _data = data;
            _chunk = chunk;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _data.Length;
        public override long Position { get => _pos; set => throw new NotSupportedException(); }
        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_pos >= _data.Length) return 0;
            var n = Math.Min(_chunk, Math.Min(count, _data.Length - _pos));
            Array.Copy(_data, _pos, buffer, offset, n);
            _pos += n;
            return n;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            if (_pos >= _data.Length) return 0;
            var n = Math.Min(_chunk, Math.Min(buffer.Length, _data.Length - _pos));
            _data.AsMemory(_pos, n).CopyTo(buffer);
            _pos += n;
            return n;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    [Fact]
    public async Task ToByteArrayAsync_ShortRead_ReadsFully()
    {
        var data = Enumerable.Range(0, 300).Select(i => (byte)(i % 251)).ToArray();
        await using var stream = new TrickleStream(data, 7);

        var result = await stream.ToByteArrayAsync();

        Assert.Equal(data.Length, result.Length);
        Assert.Equal(data, result);
    }

    [Fact]
    public async Task ContentsEqualAsync_SameLargeContent_ReturnsTrue()
    {
        // >2048 字节，覆盖修复前"只比首块即 true"的分界
        var data = new byte[6000];
        new Random(42).NextBytes(data);

        await using var a = new MemoryStream(data);
        await using var b = new MemoryStream(data);

        Assert.True(await a.ContentsEqualAsync(b));
    }

    [Fact]
    public async Task ContentsEqualAsync_DifferentContent_ReturnsFalse()
    {
        var data = new byte[6000];
        new Random(42).NextBytes(data);
        var other = (byte[])data.Clone();
        other[^1] = (byte)(other[^1] ^ 0xFF); // 尾部不同，修复前会漏检

        await using var a = new MemoryStream(data);
        await using var b = new MemoryStream(other);

        Assert.False(await a.ContentsEqualAsync(b));
    }

    [Fact]
    public async Task ContentsEqualAsync_ShortDifferent_ReturnsFalse()
    {
        await using var a = new MemoryStream("hello"u8.ToArray());
        await using var b = new MemoryStream("hella"u8.ToArray());

        Assert.False(await a.ContentsEqualAsync(b));
    }
}
