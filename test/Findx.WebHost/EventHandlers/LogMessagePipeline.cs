using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Messaging;

namespace Findx.WebHost.EventHandlers
{
    /// <summary>
    /// 消息通道
    /// </summary>
    public class LogMessagePipeline : IMessagePipeline<CancelOrderCommand, string>, IScopeDependency
    {
        public async Task<string> HandleAsync(CancelOrderCommand request, MessageHandlerDelegate<string> next, CancellationToken cancellationToken)
        {
            Console.WriteLine($"RequestAOPLog请求:{request.ToJson()}");
            var result = await next().ConfigureAwait(false);
            Console.WriteLine($"ResponseAOPLog报文:{result.ToJson()}");
            return result;
        }
    }
}

