using System;
using System.Text.Json.Serialization;

namespace Findx.EventBus
{
    /// <summary>
    /// 事件数据基类
    /// </summary>
    public abstract class EventDataBase: IEventData
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public EventDataBase()
        {
            Id = Findx.Utils.SnowflakeId.Default().NextId();
            OccurredOn = DateTime.Now;
        }
        
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="id">事件编号</param>
        /// <param name="occurredOn">事件发生时间</param>
        [JsonConstructor]
        public EventDataBase(long id, DateTime occurredOn)
        {
            Id = id;
            OccurredOn = occurredOn;
        }
        
        /// <summary>
        /// 事件编号
        /// </summary>
        [JsonInclude]
        public long Id { get; }
        
        /// <summary>
        /// 事件发生时间
        /// </summary>
        [JsonInclude]
        public DateTime OccurredOn { get; }
    }
}