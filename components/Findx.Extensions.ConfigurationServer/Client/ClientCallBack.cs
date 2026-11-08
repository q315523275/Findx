using System.Collections.Concurrent;
using Findx.Extensions.ConfigurationServer.Dtos;

namespace Findx.Extensions.ConfigurationServer.Client;

/// <summary>
///     客户端回调管理
/// </summary>
public class ClientCallBack : IClientCallBack
{
    //  双层并发字典：外层 appId-environment → 内层 traceIdentifier → 回调信息
    //  消除原 check-then-act 竞态与 List 并发 Add 损坏；外层集合不删除（GetOrAdd 复用），避免并发下丢失新注册
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ClientCallBackInfo<ConfigDataChangeDto>>> _callBacks = new();

    /// <summary>
    ///     创建回调任务
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="traceIdentifier"></param>
    /// <param name="clientIpAddress"></param>
    /// <param name="timeout"></param>
    /// <param name="requestAborted"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public Task<ConfigDataChangeDto> NewCallBackTaskAsync(string appId, string traceIdentifier, string clientIpAddress, int timeout, CancellationToken requestAborted = default)
    {
        if (timeout <= 0) throw new ArgumentOutOfRangeException(nameof(timeout), "timeout 必须大于 0");

        var clients = _callBacks.GetOrAdd(appId, _ => new ConcurrentDictionary<string, ClientCallBackInfo<ConfigDataChangeDto>>());

        // RunContinuationsAsynchronously：SetResult/Canceled 时续体投递到线程池，
        // 变更广播线程（DumpService 调用方）不会被长轮询响应序列化+IO 同步阻塞
        var source = new TaskCompletionSource<ConfigDataChangeDto>(TaskCreationOptions.RunContinuationsAsynchronously);
        var tokenSource = new CancellationTokenSource();
        var info = new ClientCallBackInfo<ConfigDataChangeDto>
        {
            Task = source,
            TraceIdentifier = traceIdentifier,
            ClientIpAddress = clientIpAddress,
            CancellationTokenSource = tokenSource
        };

        //  顺序：先完成清理回调注册与超时计时，最后原子登记——
        //  保证 CallBack 摘组只能发生在登记完成后，不会出现"登记后继续触碰已被 Dispose 的 CTS"（ObjectDisposedException → 500）
        info.CancellationTokenRegistration = tokenSource.Token.Register(() =>
        {
            if (clients.TryRemove(traceIdentifier, out _) && !source.Task.IsCompleted)
            {
                source.TrySetException(new TimeoutException($"Call {appId} client {traceIdentifier} timeout."));
            }
            // 回调内 Dispose CTS 安全（.NET 4.6+ 明确允许），消除超时路径的资源滞留
            tokenSource.Dispose();
        });

        //  客户端请求中断清理
        if (requestAborted.CanBeCanceled)
        {
            info.AbortCancellationRegistration = requestAborted.Register(() =>
            {
                if (clients.TryRemove(traceIdentifier, out _) && !source.Task.IsCompleted)
                {
                    source.TrySetCanceled();
                }
                tokenSource.Dispose();
            });
        }

        tokenSource.CancelAfter(timeout * 1000);

        //  原子登记：重复 traceIdentifier 直接失败（消除 check-then-act）；失败时先清理已注册资源再抛，不残留计时器/闭包
        if (!clients.TryAdd(traceIdentifier, info))
        {
            info.CancellationTokenRegistration.Unregister();
            info.CancellationTokenRegistration.Dispose();
            info.AbortCancellationRegistration.Unregister();
            info.AbortCancellationRegistration.Dispose();
            tokenSource.Dispose();
            throw new ArgumentException($"TraceIdentifier {traceIdentifier} callback already registered for '{appId}'", nameof(traceIdentifier));
        }

        return source.Task;
    }

    /// <summary>
    ///     设置回调任务结果
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="content"></param>
    public void CallBack(string appId, ConfigDataChangeDto content)
    {
        //  原子摘除整组：并发新注册若已进入该组则一并回执（其注册早于变更送达，语义正确）；摘除后的新注册等待下一次变更
        if (_callBacks.TryRemove(appId, out var clients))
        {
            foreach (var client in clients.Values)
            {
                client.CancellationTokenRegistration.Unregister();
                client.CancellationTokenRegistration.Dispose();
                client.AbortCancellationRegistration.Unregister();
                client.AbortCancellationRegistration.Dispose();
                client.CancellationTokenSource.Dispose();

                if (!client.Task.Task.IsCompleted)
                {
                    client.Task.SetResult(content);
                }
            }
        }
    }

    /// <summary>
    ///     客户端信息（快照副本，不暴露内部结构）
    /// </summary>
    public IDictionary<string, List<ClientCallBackInfo<ConfigDataChangeDto>>> Metrics()
    {
        return _callBacks.ToDictionary(x => x.Key, x => x.Value.Values.ToList());
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public void Dispose()
    {
        foreach (var callBack in _callBacks)
        foreach (var client in callBack.Value.Values)
        {
            client.CancellationTokenRegistration.Unregister();
            client.CancellationTokenRegistration.Dispose();
            
            client.AbortCancellationRegistration.Unregister();
            client.AbortCancellationRegistration.Dispose();
            
            client.CancellationTokenSource.Dispose();

            if (!client.Task.Task.IsCompleted) client.Task.TrySetCanceled();
        }
    }
}
