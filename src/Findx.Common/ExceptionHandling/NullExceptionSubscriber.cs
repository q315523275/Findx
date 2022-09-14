using System.Threading.Tasks;
namespace Findx.ExceptionHandling
{
    /// <summary>
    /// Ctor
    /// </summary>
    public class NullExceptionSubscriber : ExceptionSubscriber
    {
        /// <summary>
        /// 订阅处理
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public override Task HandleAsync(ExceptionNotificationContext context, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }
    }
}
