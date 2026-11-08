using System.Net.WebSockets;
using Findx.WebSocketCore.Abstractions;
using Findx.WebSocketCore.Implementation;

namespace Findx.UnitTests;

/// <summary>
///     WebSocketSessionManager 并发回归测试（修复：check-then-act 覆盖丢会话、内层 List 并发损坏、空组 key 泄漏）
/// </summary>
public class WebSocketSessionManagerTests
{
    private sealed class FakeWebSocketSession : IWebSocketSession
    {
        public long Id { get; init; }
        public string RemoteIpAddress { get; set; }
        public int RemotePort { get; set; }
        public string UserName { get; set; }
        public WebSocketState State => WebSocketState.Open;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public Task<ValueWebSocketReceiveResult> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
            => Task.FromResult(new ValueWebSocketReceiveResult(0, WebSocketMessageType.Text, true));

        public Task SendAsync(ReadOnlyMemory<byte> payload, WebSocketMessageType webSocketMessageType = WebSocketMessageType.Text, bool endOfMessage = true, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    [Fact]
    public async Task AddSession_SameUserConcurrent_NoSessionLost()
    {
        var manager = new WebSocketSessionManager();
        const int total = 4000;

        // 8 线程并发为同一用户登记不同会话 Id（修复前 check-then-act 会覆盖丢失）
        var tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
        {
            for (var i = 0; i < total / 8; i++)
            {
                var id = Interlocked.Increment(ref _counter);
                manager.AddSession(new FakeWebSocketSession { Id = id, UserName = "user1" });
            }
        }));

        await Task.WhenAll(tasks);

        Assert.Equal(4000, manager.GetAllSessions().Count());
        Assert.Equal(1, manager.Count);
    }

    [Fact]
    public async Task AddAndRemove_Concurrent_NoException()
    {
        var manager = new WebSocketSessionManager();
        const int total = 2000;

        var addTask = Task.Run(() =>
        {
            for (var i = 0; i < total; i++)
            {
                manager.AddSession(new FakeWebSocketSession { Id = i, UserName = $"user{i % 10}" });
            }
        });

        // 并发枚举快照不应抛 Collection was modified
        var enumTask = Task.Run(() =>
        {
            for (var i = 0; i < 200; i++) _ = manager.GetAllSessions().ToArray();
        });

        await Task.WhenAll(addTask, enumTask);
        await manager.RemoveSessionAsync("user0");

        Assert.Equal(9, manager.Count);
    }

    [Fact]
    public void AddToGroup_RemoveEmptyGroup_ClearsKey()
    {
        var manager = new WebSocketSessionManager();
        var session = new FakeWebSocketSession { Id = 1, UserName = "u1" };

        manager.AddToGroup(session, "g1");
        Assert.Single(manager.GetAllUserFromGroup("g1"));

        manager.RemoveFromGroup(session, "g1");
        // 组删空后空组 key 应被清理（修复前空 List 永留累积）
        Assert.Null(manager.GetAllUserFromGroup("g1"));
    }

    private static int _counter;
}
