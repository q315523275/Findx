using System.Net.WebSockets;
using System.Reflection;
using Findx.WebSocketCore;

namespace Findx.Saas.WebHost.WebShell;

public class WebSocketHandler: WebSocketHandlerBase
{
    public WebSocketHandler(IWebSocketClientManager clientManager, IWebSocketSerializer serializer) : base(clientManager, serializer)
    {
    }
    /// <summary>
    /// 接收消息
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="result"></param>
    /// <param name="receivedMessage"></param>
    public override async Task ReceiveAsync(WebSocketClient socket, WebSocketReceiveResult result, string receivedMessage)
    {
        try
        {
            await foreach(var item in ProcessX.StartAsync(receivedMessage))
            {
                await SendMessageAsync(socket, new WebSocketMessage<string> { MessageType = MessageType.Text, Data = item }).ConfigureAwait(false);
            }
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