using System.Collections.Concurrent;
using Findx.Extensions.ConfigurationServer.Dtos;

namespace Findx.Extensions.ConfigurationServer.Client;

/// <summary>
///     客户端回调管理
/// </summary>
public class ClientCallBack : IClientCallBack
{
    /// <summary>
    ///     客户端连接字典
    /// </summary>
    private readonly ConcurrentDictionary<string, List<ClientCallBackInfo<ConfigDataChangeDto>>> _callBacks = new();

    /// <summary>
    ///     创建回调任务
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="traceIdentifier"></param>
    /// <param name="clientIpAddress"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Task<ConfigDataChangeDto> NewCallBackTaskAsync(string appId, string traceIdentifier, string clientIpAddress, int timeout)
    {
        if (!_callBacks.ContainsKey(appId))
            _callBacks.TryAdd(appId, []);

        if (_callBacks[appId].Any(x => x.TraceIdentifier == traceIdentifier))
            throw new ArgumentException($"TraceIdentifier {traceIdentifier} callback already registered for '{appId}'", nameof(traceIdentifier));

        var source = new TaskCompletionSource<ConfigDataChangeDto>();
        var tokenSource = new CancellationTokenSource();
        var tokenRegister = tokenSource.Token.Register(() =>
        {
            if (_callBacks.TryGetValue(appId, out var clients) && clients.Any(x => x.TraceIdentifier == traceIdentifier))
            {
                clients.RemoveAll(x => x.TraceIdentifier == traceIdentifier);
                if (!clients.Any()) _callBacks.TryRemove(appId, out _);
            }

            if (!source.Task.IsCompleted)
                source.TrySetException(new TimeoutException($"Call {appId} client {traceIdentifier} timeout."));
        });

        _callBacks[appId].Add(new ClientCallBackInfo<ConfigDataChangeDto> { Task = source, TraceIdentifier = traceIdentifier, ClientIpAddress = clientIpAddress, CancellationTokenSource = tokenSource, CancellationTokenRegistration = tokenRegister });

        tokenSource.CancelAfter(timeout * 1000);

        return source.Task;
    }

    /// <summary>
    ///     设置回调任务结果
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="content"></param>
    public void CallBack(string appId, ConfigDataChangeDto content)
    {
        if (_callBacks.TryGetValue(appId, out var clients))
        {
            foreach (var client in clients)
            {
                client.CancellationTokenRegistration.Unregister();
                client.CancellationTokenRegistration.Dispose();
                client.CancellationTokenSource.Dispose();
                
                if (!client.Task.Task.IsCompleted)
                    client.Task.SetResult(content);
            }

            _callBacks.TryRemove(appId, out _);
        }
    }

    /// <summary>
    ///     客户端信息
    /// </summary>
    /// <returns></returns>
    public IDictionary<string, List<ClientCallBackInfo<ConfigDataChangeDto>>> Metrics()
    {
        return _callBacks;
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public void Dispose()
    {
        foreach (var callBack in _callBacks)
        foreach (var client in callBack.Value)
        {
            client.CancellationTokenRegistration.Unregister();
            client.CancellationTokenRegistration.Dispose();
            client.CancellationTokenSource.Dispose();
            
            if (!client.Task.Task.IsCompleted) client.Task.TrySetCanceled();
        }
    }
}