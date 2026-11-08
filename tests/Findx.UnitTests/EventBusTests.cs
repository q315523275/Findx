using Findx.Caching;
using Findx.Events;

namespace Findx.UnitTests;

/// <summary>
///     EventBus 回归测试（修复：集成事件未注册返回 null Task 导致 await NRE → 显式抛 InvalidOperationException）
/// </summary>
public class EventBusTests
{
    private sealed class FakeIntegrationEvent : IIntegrationEvent;

    [Fact]
    public async Task PublishAsync_IntegrationEvent_NoDistributedBus_Throws()
    {
        var bus = new EventBus(null!, null!, null);

        // 修复前返回 null Task → await 处 NullReferenceException（且语义含糊）；现在必须显式抛 InvalidOperationException
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => bus.PublishAsync(new FakeIntegrationEvent()));
        Assert.Contains("IDistributedEventBus", ex.Message);
    }

    [Fact]
    public async Task PublishAsync_NullEvent_ThrowsArgumentNull()
    {
        var bus = new EventBus(null!, null!, null);

        await Assert.ThrowsAsync<ArgumentNullException>(() => bus.PublishAsync<FakeIntegrationEvent>(null!));
    }
}
