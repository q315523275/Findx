using System;
using System.Collections.Generic;

namespace Findx.Tasks.Scheduling
{
    /// <summary>
    /// 调度任务任务上下文
    /// </summary>
    public class TaskContext : ITaskContext
    {
        public TaskContext(IServiceProvider serviceProvider, IDictionary<string, string> parameter, Guid taskId)
        {
            ServiceProvider = serviceProvider;
            Parameter = parameter;
            TaskId = taskId;
        }

        /// <summary>
        /// 服务容器提供者
        /// </summary>
        public IServiceProvider ServiceProvider { get; private set; }
        /// <summary>
        /// 任务参数
        /// </summary>
        public IDictionary<string, string> Parameter { get; set; }
        /// <summary>
        /// 当前分片序号，执行器集群列表中当前执行器的序号
        /// </summary>
        public int ShardIndex { get; set; }
        /// <summary>
        /// 总分片数，执行器集群的总机器数量
        /// </summary>
        public int ShardTotal { get; set; }

        public Guid TaskId { get; set; }
    }
}
