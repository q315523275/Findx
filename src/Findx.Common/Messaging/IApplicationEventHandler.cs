using System;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Messaging
{
	/// <summary>
    /// 应用事件处理器
    /// </summary>
	public interface IApplicationEventHandler<in TEvent> where TEvent : IApplicationEvent
    {
        /// <summary>
        /// 处理泛型知消消息
        /// </summary>
        /// <param name="applicationEvent"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task HandleAsync(TEvent applicationEvent, CancellationToken cancellationToken = default);
	}
}
