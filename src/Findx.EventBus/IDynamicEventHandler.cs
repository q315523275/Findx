using System.Threading.Tasks;

namespace Findx.EventBus
{
    /// <summary>
    /// 动态事件处理器
    /// </summary>
    public interface IDynamicEventHandler
    {
        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns></returns>
        Task HandleAsync(string eventData);
    }
}
