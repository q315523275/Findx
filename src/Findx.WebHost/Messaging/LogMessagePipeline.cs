using Findx.Data;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Messaging;
using Findx.WebHost.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.WebHost.Messaging
{
    public class LogMessagePipeline : IMessagePipeline<CancelOrderCommand, bool>, IScopeDependency
    {
        public async Task<bool> Handle(CancelOrderCommand request, MessageHandlerDelegate<bool> next, CancellationToken cancellationToken)
        {
            Console.WriteLine($"RequestLog请求:{request.ToJson()}");
            var result = await next().ConfigureAwait(false);
            Console.WriteLine($"ResponseLog报文:{result.ToJson()}");
            return result;
        }
    }
}
