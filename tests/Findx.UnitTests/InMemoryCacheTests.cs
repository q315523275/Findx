using Findx.Caching;
using Findx.Threading;

namespace Findx.UnitTests;

/// <summary>
///     InMemoryCache 并发回归测试（修复：GetOrAddItem do-while TryAdd 首写孤儿对象丢计数 → AddOrUpdate + ReferenceEquals 计写）
/// </summary>
public class InMemoryCacheTests
{
    [Fact]
    public void Increment_ConcurrentSameKey_NoLostUpdate()
    {
        var timer = new AsyncTimer();
        using var cache = new InMemoryCache(timer);
        const int total = 10000;
        const int threads = 8;

        var tasks = Enumerable.Range(0, threads).Select(_ => Task.Run(() =>
        {
            for (var i = 0; i < total / threads; i++) cache.Increment("counter");
        }));

        Task.WaitAll(tasks.ToArray());

        // 修复前并发首写时部分线程作用在孤儿 AtomicLong 上，最终计数 < total
        Assert.Equal(total, cache.Increment("counter", 0));
    }

    [Fact]
    public void Increment_AsyncConcurrentSameKey_NoLostUpdate()
    {
        var timer = new AsyncTimer();
        using var cache = new InMemoryCache(timer);
        const int total = 10000;
        const int threads = 8;

        var tasks = Enumerable.Range(0, threads).Select(_ => Task.Run(async () =>
        {
            for (var i = 0; i < total / threads; i++) await cache.IncrementAsync("counter");
        }));

        Task.WaitAll(tasks.ToArray());

        Assert.Equal(total, cache.Increment("counter", 0));
    }

    [Fact]
    public async Task GetAsync_AfterAdd_ReturnsValue()
    {
        var timer = new AsyncTimer();
        using var cache = new InMemoryCache(timer);
        await cache.AddAsync("k", 42);

        Assert.Equal(42, await cache.GetAsync<int>("k"));
    }
}
