using System;

namespace Findx.EventBus
{
    /// <summary>
    /// 事件存储消息
    /// </summary>
    public class EventMediumMessage
    {
        /// <summary>
        /// 事件编号
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// 事件名
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 执行次数
        /// </summary>
        public int TryCount { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public EventStatus Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 过期事件
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }
}
