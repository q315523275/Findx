using System.Threading.Tasks;

namespace Findx.EventBus
{
    /// <summary>
    ///     事件调度器
    /// </summary>
    public interface IEventDispatcher
    {
        /// <summary>
        ///     入队发布
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        ValueTask EnqueueToPublish(Message message);
    }
}