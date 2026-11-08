using System.Collections.Concurrent;
using Findx.Utilities;

namespace Findx.UnitTests;

/// <summary>
///     SnowflakeIdUtility 回归测试（审查通过项固化：并发唯一 + TryParse 字段回读）
/// </summary>
public class SnowflakeIdUtilityTests
{
    [Fact]
    public void NextId_Concurrent_NoDuplicate()
    {
        var generator = SnowflakeIdUtility.Default();
        var ids = new ConcurrentDictionary<long, byte>();
        const int threads = 8;
        const int perThread = 2000;

        var tasks = Enumerable.Range(0, threads).Select(_ => Task.Run(() =>
        {
            for (var i = 0; i < perThread; i++)
            {
                var id = generator.NextId();
                if (!ids.TryAdd(id, 0)) throw new InvalidOperationException($"重复 ID: {id}");
            }
        }));

        Task.WaitAll(tasks.ToArray());
        Assert.Equal(threads * perThread, ids.Count);
    }

    [Fact]
    public void NextId_ThenTryParse_RoundTrips()
    {
        var generator = new SnowflakeIdUtility(7);
        var before = DateTime.Now.AddSeconds(-1);

        var id = generator.NextId();
        var after = DateTime.Now.AddSeconds(1);

        Assert.True(SnowflakeIdUtility.TryParse(id, out var time, out var workerId, out var sequence));
        Assert.Equal(7, workerId);
        Assert.True(time >= before && time <= after, $"生成时间应在范围内，实际 {time:O}");
        Assert.True(sequence >= 0);
    }

    [Fact]
    public void NextId_MonotonicallyIncreasing()
    {
        var generator = SnowflakeIdUtility.Default();
        var previous = generator.NextId();
        for (var i = 0; i < 1000; i++)
        {
            var current = generator.NextId();
            Assert.True(current > previous);
            previous = current;
        }
    }
}
