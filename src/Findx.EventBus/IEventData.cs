using System;

namespace Findx.EventBus
{
    /// <summary>
    /// 事件内容
    /// </summary>
    public interface IEventData
    {
        /// <summary>
        /// 编号
        /// </summary>
        long Id { get; }

        /// <summary>
        /// 发生时间
        /// </summary>
        DateTime OccurredOn { get; }
    }
}