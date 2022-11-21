using Findx.DependencyInjection;
using System.Threading.Tasks;
namespace Findx.ExceptionHandling
{
    /// <summary>
    /// 异常订阅器
    /// </summary>
    [MultipleDependency]
    public interface IExceptionSubscriber
    {
        /// <summary>
        /// 排序号,正序
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="context">异常通知上下文</param>
        /// <param name="token"></param>
        Task HandleAsync(ExceptionNotificationContext context, CancellationToken token = default);
    }
}
