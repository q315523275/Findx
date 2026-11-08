using Findx.Extensions.ConfigurationServer.Client;
using Findx.Extensions.ConfigurationServer.Dtos;

namespace Findx.UnitTests;

/// <summary>
///     ClientCallBack 并发回归测试：注册→变更唤醒 / 超时 / 客户端断开 / 重复 traceId / timeout 校验 / 摘组语义 / 快照
/// </summary>
public class ClientCallBackTests
{
    private static ConfigDataChangeDto NewChange(string dataId = "d1") => new()
    {
        AppId = "app", Environment = "prod", DataId = dataId, DataType = "text", Content = "v1", Version = 2
    };

    // ---------- 主路径 ----------

    [Fact]
    public async Task NewCallBackTaskAsync_ThenCallBack_CompletesWithSameContent()
    {
        using var callBack = new ClientCallBack();
        var task = callBack.NewCallBackTaskAsync("app-prod", "trace-1", "127.0.0.1", 30);

        var content = NewChange();
        callBack.CallBack("app-prod", content);

        var result = await task;
        Assert.Same(content, result);
    }

    [Fact]
    public async Task NewCallBackTaskAsync_MultipleClients_CallBackCompletesAll()
    {
        using var callBack = new ClientCallBack();
        var t1 = callBack.NewCallBackTaskAsync("app-prod", "trace-1", "127.0.0.1", 30);
        var t2 = callBack.NewCallBackTaskAsync("app-prod", "trace-2", "127.0.0.1", 30);

        var content = NewChange();
        callBack.CallBack("app-prod", content);

        var results = await Task.WhenAll(t1, t2);
        Assert.All(results, r => Assert.Same(content, r));
    }

    [Fact]
    public async Task NewCallBackTaskAsync_DifferentAppId_Isolated()
    {
        using var callBack = new ClientCallBack();
        var t1 = callBack.NewCallBackTaskAsync("app-prod", "trace-1", "127.0.0.1", 30);
        var t2 = callBack.NewCallBackTaskAsync("other-prod", "trace-1", "127.0.0.1", 30);

        callBack.CallBack("app-prod", NewChange());

        await t1;                                    // app-prod 完成
        Assert.False(t2.IsCompleted);                // other-prod 不受影响
        callBack.CallBack("other-prod", NewChange("d2"));
        await t2;
    }

    // ---------- 超时 ----------

    [Fact]
    public async Task NewCallBackTaskAsync_Timeout_ThrowsTimeoutException()
    {
        using var callBack = new ClientCallBack();
        var task = callBack.NewCallBackTaskAsync("app-prod", "trace-1", "127.0.0.1", 1);

        await Assert.ThrowsAsync<TimeoutException>(() => task);
    }

    [Fact]
    public async Task CallBack_AfterTimeout_NoThrowAndTaskAlreadyComplete()
    {
        using var callBack = new ClientCallBack();
        var task = callBack.NewCallBackTaskAsync("app-prod", "trace-1", "127.0.0.1", 1);

        await Assert.ThrowsAsync<TimeoutException>(() => task);
        // 超时后变更送达：组已被超时清理摘除，CallBack 为空操作，不抛异常、不覆盖已超时任务
        callBack.CallBack("app-prod", NewChange());
        Assert.True(task.IsFaulted);
    }

    // ---------- 客户端断开 ----------

    [Fact]
    public async Task NewCallBackTaskAsync_RequestAborted_Cancels()
    {
        using var callBack = new ClientCallBack();
        using var cts = new CancellationTokenSource();
        var task = callBack.NewCallBackTaskAsync("app-prod", "trace-1", "127.0.0.1", 30, cts.Token);

        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
    }

    // ---------- 参数与重复校验 ----------

    [Fact]
    public async Task NewCallBackTaskAsync_DuplicateTraceIdentifier_ThrowsArgumentException()
    {
        using var callBack = new ClientCallBack();
        // 首次注册成功即可（不 await——等待任务会等到 30s 超时；Dispose 会取消未完成任务）
        _ = callBack.NewCallBackTaskAsync("app-prod", "trace-1", "127.0.0.1", 30);

        // 同 appId 同 traceIdentifier 二次注册：TryAdd 原子拒绝，同步抛 ArgumentException
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            callBack.NewCallBackTaskAsync("app-prod", "trace-1", "127.0.0.1", 30));
        Assert.Contains("trace-1", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task NewCallBackTaskAsync_NonPositiveTimeout_ThrowsArgumentOutOfRangeException(int timeout)
    {
        using var callBack = new ClientCallBack();
        // timeout<=0 在方法体同步部分抛出，ThrowsAsync 同样可捕获
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            callBack.NewCallBackTaskAsync("app-prod", "trace-1", "127.0.0.1", timeout));
    }

    // ---------- 摘组语义 ----------

    [Fact]
    public async Task CallBack_BeforeRegistration_NewRegistrationWaitsForNextChange()
    {
        using var callBack = new ClientCallBack();
        // 变更先于注册送达：组不存在/已摘除，本次变更无人回执（客户端下次全量拉取兜底）
        callBack.CallBack("app-prod", NewChange());

        var task = callBack.NewCallBackTaskAsync("app-prod", "trace-1", "127.0.0.1", 30);
        await Task.Delay(150);
        Assert.False(task.IsCompleted);                       // 不因早到的变更而完成

        callBack.CallBack("app-prod", NewChange("d2"));       // 下一次变更才唤醒
        var result = await task;
        Assert.Equal("d2", result.DataId);
    }

    // ---------- 快照 ----------

    [Fact]
    public void Metrics_ReturnsCountSnapshot()
    {
        using var callBack = new ClientCallBack();
        callBack.NewCallBackTaskAsync("app-prod", "trace-1", "127.0.0.1", 30);
        callBack.NewCallBackTaskAsync("app-prod", "trace-2", "127.0.0.1", 30);
        callBack.NewCallBackTaskAsync("other-prod", "trace-3", "127.0.0.1", 30);

        var metrics = callBack.Metrics();
        Assert.Equal(2, metrics.Count);
        Assert.Equal(2, metrics["app-prod"].Count);
        Assert.Single(metrics["other-prod"]);

        callBack.CallBack("app-prod", NewChange());            // 摘组后快照收敛
        metrics = callBack.Metrics();
        Assert.False(metrics.ContainsKey("app-prod"));
    }

    // ---------- 并发 ----------

    [Fact]
    public async Task Concurrent_RegisterAndCallBack_AllTasksCompleteWithoutThrow()
    {
        using var callBack = new ClientCallBack();
        const int clients = 20;
        const int batches = 5;

        var tasks = new List<Task<ConfigDataChangeDto>>();
        for (var batch = 0; batch < batches; batch++)
        {
            var appKey = $"app-prod-{batch}";
            for (var i = 0; i < clients; i++)
                tasks.Add(callBack.NewCallBackTaskAsync(appKey, $"trace-{batch}-{i}", "127.0.0.1", 30));
            callBack.CallBack(appKey, NewChange($"b{batch}"));
        }

        // 全部任务必须在无异常下终结（成功拿到变更，或注册晚于摘组而落入等待——此处必有变更送达）
        var results = await Task.WhenAll(tasks);
        Assert.Equal(batches * clients, results.Length);
        Assert.All(results, r => Assert.NotNull(r));
    }
}
