using System;

namespace Findx.EventBus.Attribute
{
    /// <summary>
    ///     主题属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TopicAttribute : System.Attribute
    {
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isPartial"></param>
        public TopicAttribute(string name, bool isPartial = false)
        {
            Name = name;
            IsPartial = isPartial;
        }

        /// <summary>
        ///     主题名称
        ///     <remarks>
        ///         针对RabbitMq->route key
        ///     </remarks>
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     是否分区
        /// </summary>
        public bool IsPartial { get; }

        /// <summary>
        ///     组名
        ///     <remarks>
        ///         针对RabbitMq->queue.name
        ///     </remarks>
        /// </summary>
        public string Group { get; set; } = default!;
    }
}