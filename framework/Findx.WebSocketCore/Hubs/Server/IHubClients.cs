using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.WebSocketCore.Hubs.Server;

/// <summary>
///     提供对客户端连接的访问的抽象。
/// </summary>
/// <typeparam name="TClient"></typeparam>
public interface IHubClients<TClient>
{
    /// <summary>
    ///     客户端连接总数
    /// </summary>
    int Count { get; }

    /// <summary>
    ///     获取并返回 所有客户端连接
    /// </summary>
    /// <returns></returns>
    IEnumerable<TClient> GetAllClients();

    /// <summary>
    ///     获取并返回 所有组内客户端连接
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    IEnumerable<string> GetAllClientsFromGroup(string groupId);

    /// <summary>
    ///     添加 客户端连接
    /// </summary>
    /// <param name="client"></param>
    void AddClient(TClient client);

    /// <summary>
    ///     移除 客户端连接
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RemoveClientAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     移除 客户端连接
    /// </summary>
    /// <param name="client"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RemoveClientAsync(TClient client, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     添加 客户端连接分组 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="groupId"></param>
    void AddToGroup(TClient client, string groupId);

    /// <summary>
    ///     移除 指定组内的客户端连接
    /// </summary>
    /// <param name="client"></param>
    /// <param name="groupId"></param>
    void RemoveFromGroup(TClient client, string groupId);
}