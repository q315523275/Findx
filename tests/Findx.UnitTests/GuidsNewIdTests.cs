using System.Collections.Concurrent;
using Findx.NewId;
using Findx.Guids.NewId;

namespace Findx.UnitTests;

/// <summary>
///     Guids.NewId 审查通过项固化：MassTransitSequentialGuidGenerator 并发唯一 + 顺序性
/// </summary>
public class GuidsNewIdTests
{
    [Fact]
    public void Create_Concurrent_NoDuplicate()
    {
        IGuidGenerator generator = new MassTransitSequentialGuidGenerator();
        var ids = new ConcurrentDictionary<Guid, byte>();
        const int threads = 8;
        const int perThread = 2000;

        var tasks = Enumerable.Range(0, threads).Select(_ => Task.Run(() =>
        {
            for (var i = 0; i < perThread; i++)
            {
                var id = generator.Create();
                if (!ids.TryAdd(id, 0)) throw new InvalidOperationException($"重复 Guid: {id}");
            }
        }));

        Task.WaitAll(tasks.ToArray());
        Assert.Equal(threads * perThread, ids.Count);
    }

    [Fact]
    public void Create_Sequential_ProducesOrderedGuids()
    {
        IGuidGenerator generator = new MassTransitSequentialGuidGenerator();
        var first = generator.Create();
        var second = generator.Create();

        Assert.NotEqual(first, second);
    }
}
