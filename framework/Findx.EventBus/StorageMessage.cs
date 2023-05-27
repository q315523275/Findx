using System;
using Findx.Data;

namespace Findx.EventBus
{
    /// <summary>
    ///     用于存储的消息
    /// </summary>
    public class StorageMessage : EntityBase<long>
    {
        /// <summary>
        ///     消息名称
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        ///     消息分组
        /// </summary>
        public virtual string Group { get; set; }

        /// <summary>
        ///     消息内容
        /// </summary>
        public virtual string Content { get; set; } = default!;

        /// <summary>
        ///     发生时间
        /// </summary>
        public virtual DateTime OccurredOn { get; set; }

        /// <summary>
        ///     到期时间
        /// </summary>
        public virtual DateTime? ExpiresAt { get; set; }

        /// <summary>
        ///     状态名称
        /// </summary>
        public virtual string StatusName { get; set; }

        /// <summary>
        ///     重试次数
        /// </summary>
        public virtual int Retries { get; set; }

        /// <summary>
        ///     创建时间
        /// </summary>
        public virtual DateTime CreatedTime { get; set; }
    }
}