using System;
using System.Collections.Generic;

namespace Findx.Tasks.Scheduling
{
    /// <summary>
    /// 任务执行上下文
    /// </summary>
    public interface ITaskContext
    {
        /// <summary>
        /// 服务提供器
        /// </summary>
        IServiceProvider ServiceProvider { get; }
        /// <summary>
        /// 任务编号
        /// </summary>
        Guid TaskId { get; }
        /// <summary>
        /// 任务编号
        /// </summary>
        IDictionary<string, object> Parameter { get; }
        /// <summary>
        /// 任务分片索引
        /// </summary>
        int ShardIndex { get; }
        /// <summary>
        /// 任务分片总数
        /// </summary>
        int ShardTotal { get; }
    }
}
