using System.Threading.Tasks;

namespace Findx.ExceptionHandling
{
    /// <summary>
    /// 异常订阅器基类
    /// </summary>
    public abstract class ExceptionSubscriber : IExceptionSubscriber
    {
        /// <summary>
        /// 执行排序号
        /// </summary>
        public virtual int Order => 10;

        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="context">异常通知上下文</param>
        /// <param name="token"></param>
        public abstract Task HandleAsync(ExceptionNotificationContext context, CancellationToken token = default);
    }
}
