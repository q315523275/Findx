using System.Net.WebSockets;
using Findx.Extensions;
using Findx.WebSocketCore;
using Findx.WebSocketCore.Abstractions;
using Findx.WebSocketCore.Extensions;

namespace WebApplication1;

public class WebSocketHandler : WebSocketHandlerBase
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="webSocketSessionManager"></param>
    public WebSocketHandler(IWebSocketSessionManager webSocketSessionManager) : base(webSocketSessionManager)
    {
    }

    /// <summary>
    ///     接收消息
    /// </summary>
    /// <param name="client"></param>
    /// <param name="message"></param>
    /// <param name="result"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task ReceiveAsync(IWebSocketSession client, ResponseMessage message, ValueWebSocketReceiveResult result, CancellationToken cancellationToken = default)
    {
        // if (message.MessageType == WebSocketMessageType.Text && message.Text.IsNotNullOrWhiteSpace())
        // {
        //     await foreach (var item in ProcessX.StartAsync(message.Text))
        //     {
        //         await SendMessageAsync(client, new RequestTextMessage(item), cancellationToken).ConfigureAwait(false);
        //     }
        // }
        var text = await message.AsTextAsync(cancellationToken).ConfigureAwait(false);
        await SendMessageAsync(client, new RequestTextMessage(text), cancellationToken).ConfigureAwait(false);
    }
}