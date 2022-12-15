using System.Net.WebSockets;
using System.Reflection;
using Findx.WebSocketCore;

namespace Findx.Admin.WebHost.WebShell;

public class WebSocketHandler: WebSocketHandlerBase
{
    public WebSocketHandler(WebSocketConnectionManager webSocketConnectionManager, IWebSocketSerializer serializer) : base(webSocketConnectionManager, serializer)
    {
    }
    /// <summary>
    /// 接收消息
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="result"></param>
    /// <param name="receivedMessage"></param>
    public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, WebSocketMessage receivedMessage)
    {
        try
        {
            var res = await Findx.ProcessX.ProcessX.StartAsync(receivedMessage.Data).ToListAsync();
                
            await SendMessageAsync(socket, new WebSocketMessage<IEnumerable<string>> { MessageType = MessageType.Text, Data = res }).ConfigureAwait(false);
        }
        catch (TargetParameterCountException)
        {
            await SendMessageAsync(socket, new WebSocketMessage<string> { MessageType = MessageType.Error, Data = $"does not take parameters!" }).ConfigureAwait(false);
        }
        catch (ArgumentException)
        {
            await SendMessageAsync(socket, new WebSocketMessage<string> { MessageType = MessageType.Error, Data = $"takes different arguments!" }).ConfigureAwait(false);
        }
    }
}