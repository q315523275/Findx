using System.Threading;
using System.Threading.Tasks;

namespace Findx.Messaging
{
    /// <summary>
    /// 应用事件推送器
    /// </summary>
    public interface IApplicationEventPublisher
    {
        /// <summary>
        /// 事件推送
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="applicationEvent"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task PublishAsync<TEvent>(TEvent applicationEvent, CancellationToken cancellationToken = default) where TEvent : IApplicationEvent;
    }
}
