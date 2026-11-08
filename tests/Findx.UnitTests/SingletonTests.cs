using Findx.Data;

namespace Findx.UnitTests;

/// <summary>
///     Singleton 回归测试（修复：setter 加锁保证实例与字典两处状态一致）
/// </summary>
public class SingletonTests
{
    private sealed class Payload
    {
        public Payload(string value) => Value = value;
        public string Value { get; }
    }

    [Fact]
    public void Instance_SetAndGet_ReturnsSame()
    {
        var first = new Payload("a");
        Singleton<Payload>.Instance = first;

        Assert.Same(first, Singleton<Payload>.Instance);
    }

    [Fact]
    public void Instance_Update_ReflectsLatest()
    {
        var a = new Payload("a");
        var b = new Payload("b");

        Singleton<Payload>.Instance = a;
        Assert.Same(a, Singleton<Payload>.Instance);

        Singleton<Payload>.Instance = b;
        Assert.Same(b, Singleton<Payload>.Instance);
    }

    [Fact]
    public void Instance_ConcurrentSet_NoThrow()
    {
        var tasks = Enumerable.Range(0, 8).Select(i => Task.Run(() =>
        {
            Singleton<Payload>.Instance = new Payload($"v{i}");
        }));

        Task.WaitAll(tasks.ToArray());

        // 并发写入后读取不抛异常，且与某次写入一致
        Assert.NotNull(Singleton<Payload>.Instance);
        Assert.StartsWith("v", Singleton<Payload>.Instance.Value);
    }
}
