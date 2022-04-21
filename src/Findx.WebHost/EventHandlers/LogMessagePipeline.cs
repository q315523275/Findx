using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Messaging;

namespace Findx.WebHost.EventHandlers
{
    public class LogMessagePipeline : IMessagePipeline<CancelOrderCommand, string>, IScopeDependency
    {
        public async Task<string> Handle(CancelOrderCommand request, MessageHandlerDelegate<string> next, CancellationToken cancellationToken)
        {
            Console.WriteLine($"RequestLog请求:{request.ToJson()}");
            var result = await next().ConfigureAwait(false);
            Console.WriteLine($"ResponseLog报文:{result.ToJson()}");
            return result;
        }
    }
}

