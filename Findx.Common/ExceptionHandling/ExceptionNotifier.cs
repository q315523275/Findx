using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Findx.ExceptionHandling
{
    /// <summary>
    /// 异常通知器
    /// </summary>
    public class ExceptionNotifier : IExceptionNotifier
    {
        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger<ExceptionNotifier> _logger;

        /// <summary>
        /// 多组异常订阅器
        /// </summary>
        private readonly IEnumerable<IExceptionSubscriber> _exceptionSubscribers;

        public ExceptionNotifier(ILogger<ExceptionNotifier> logger, IEnumerable<IExceptionSubscriber> exceptionSubscribers)
        {
            _logger = logger;
            _exceptionSubscribers = exceptionSubscribers;
        }

        /// <summary>
        /// 通知
        /// </summary>
        /// <param name="context">异常通知上下文</param>
        public async Task NotifyAsync(ExceptionNotificationContext context)
        {
            Check.NotNull(context, nameof(context));

            foreach (var exceptionSubscriber in _exceptionSubscribers)
            {
                try
                {
                    await exceptionSubscriber.HandleAsync(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{exceptionSubscriber.GetType().AssemblyQualifiedName} 异常订阅器抛出异常!");
                }
            }
        }
    }
}
