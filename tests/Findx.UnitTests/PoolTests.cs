using System.Diagnostics.Metrics;
using Findx;

namespace Findx.UnitTests;

/// <summary>
///     Pool 回归测试（MemoryStream 池化：Rent/Return clear 语义，Return 补 clear 后复用状态干净）
/// </summary>
public class PoolTests
{
    [Fact]
    public void MemoryStream_RentReturnClear_Reusable()
    {
        var pool = Pool.MemoryStream;

        var ms = pool.Rent();
        Assert.NotNull(ms);
        Assert.Equal(0, ms.Length);

        ms.WriteByte(1);
        ms.WriteByte(2);
        Assert.Equal(2, ms.Length);

        pool.Return(ms, clear: true);

        // Return 后原流已回收（RecyclableMemoryStream 语义），不能再访问；再次 Rent 应得到可用流
        var next = pool.Rent();
        Assert.Equal(0, next.Length);
        next.WriteByte(3);
        Assert.Equal(1, next.Length);
        pool.Return(next, clear: true);
    }

    [Fact]
    public void MemoryStream_RentAfterReturn_Works()
    {
        var pool = Pool.MemoryStream;

        var first = pool.Rent();
        first.WriteByte(1);
        pool.Return(first, clear: true);

        var second = pool.Rent();
        // 归还后复用（可能同实例或新实例），必须可正常写入
        second.WriteByte(42);
        Assert.Equal(1, second.Length);
        pool.Return(second, clear: true);
    }
}
