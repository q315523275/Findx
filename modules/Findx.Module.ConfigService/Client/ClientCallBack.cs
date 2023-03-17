using System.Collections.Concurrent;
using Findx.Module.ConfigService.Dtos;

namespace Findx.Module.ConfigService.Client;

/// <summary>
/// 客户端回调
/// </summary>
public class ClientCallBack: IClientCallBack
{

    private readonly  ConcurrentDictionary<string, List<ClientCallBackInfo<ConfigDataChangeDto>>> _callBacks = new();

    public Task<ConfigDataChangeDto> NewCallBackTaskAsync(string appId, string reqId, string clientIp, int timeout)
    {
        if (!_callBacks.ContainsKey(appId))
        {
            _callBacks.TryAdd(appId, new List<ClientCallBackInfo<ConfigDataChangeDto>>());
        }
        
        if (_callBacks[appId].Any(x => x.ReqId == reqId))
        {
            throw new ArgumentException($"Client {reqId} callback already registered for '{appId}'", nameof(reqId));
        }
        
        var source = new TaskCompletionSource<ConfigDataChangeDto>();
        var tokenSource = new CancellationTokenSource();
        tokenSource.Token.Register(() =>
        {
            if (_callBacks.TryGetValue(appId, out var clients) && clients.Any(x => x.ReqId == reqId))
            {
                clients.RemoveAll(x => x.ReqId == reqId);
                if (!clients.Any()) _callBacks.TryRemove(appId, out _);
            }
            if (!source.Task.IsCompleted)
            {
                source.TrySetException(new TimeoutException($"Call {appId} client {reqId} timeout."));
            }
        });
        
        _callBacks[appId].Add(new ClientCallBackInfo<ConfigDataChangeDto> { Task = source, ReqId = reqId, ClientIp = clientIp, CancellationTokenSource = tokenSource });
        
        tokenSource.CancelAfter(timeout * 1000);

        return source.Task;
    }

    public void CallBack(string appId, ConfigDataChangeDto content)
    {
        if (_callBacks.TryGetValue(appId, out var clients))
        {
            foreach (var client in clients)
            {
                if (!client.Task.Task.IsCompleted)
                    client.Task.SetResult(content);
                client.CancellationTokenSource.Dispose();
            }
            _callBacks.TryRemove(appId, out _);
        }
    }

    public void Dispose()
    {
        foreach (var callBack in _callBacks)
        {
            foreach (var client in callBack.Value)
            {
                if (!client.Task.Task.IsCompleted)
                    client.Task.TrySetCanceled();
                client.CancellationTokenSource.Dispose();
            }
        }
    }
}