using System.Net.WebSockets;
using System.Reflection;
using Findx.WebSocketCore;

namespace Findx.Saas.WebHost.WebShell;

public class WebSocketHandler : WebSocketHandlerBase
{
    public WebSocketHandler(IWebSocketClientManager clientManager, IWebSocketSerializer serializer) : base(clientManager, serializer)
    {
    }
    
    /// <summary>
    ///     接收消息
    /// </summary>
    /// <param name="client"></param>
    /// <param name="result"></param>
    /// <param name="receivedMessage"></param>
    /// <param name="cancellationToken"></param>
    public override async Task ReceiveAsync(WebSocketClient client, WebSocketReceiveResult result, string receivedMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            await foreach (var item in ProcessX.StartAsync(receivedMessage))
            {
                await SendMessageAsync(client, new WebSocketMessage<string> { Type = MessageType.Text, Data = item }, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (TargetParameterCountException)
        {
            await SendMessageAsync(client, new WebSocketMessage<string> { Type = MessageType.Error, Data = "does not take parameters!" }, cancellationToken).ConfigureAwait(false);
        }
        catch (ArgumentException)
        {
            await SendMessageAsync(client, new WebSocketMessage<string> { Type = MessageType.Error, Data = "takes different arguments!" }, cancellationToken).ConfigureAwait(false);
        }
    }
}