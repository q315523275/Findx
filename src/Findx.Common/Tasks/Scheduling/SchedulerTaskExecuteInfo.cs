using System;

namespace Findx.Tasks.Scheduling
{
    /// <summary>
    /// 调度任务执行信息
    /// </summary>
    public class SchedulerTaskExecuteInfo
    {
        /// <summary>
        /// 任务执行ID
        /// </summary>
        public Guid ExecuteId { get; set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid TaskId { get; set; }

        /// <summary>
        /// 任务名
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// 调度任务任务参数
        /// </summary>
        public string TaskArgs { get; set; }

        /// <summary>
        /// 调度任务命名全路径
        /// </summary>
        public string TaskFullName { get; set; }

        /// <summary>
        /// 执行状态
        /// 0待执行
        /// 1执行中
        /// 2执行完成
        /// 3执行失败
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 执行次数
        /// </summary>
        public int TryCount { get; set; }

        /// <summary>
        /// 任务时间
        /// </summary>
        public DateTime TaskTime { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        public DateTime? ExecuteTime { get; set; }

        /// <summary>
        /// 延时执行时间
        /// 单位：秒
        /// </summary>
        public double DelayRunTime { get; set; }

        /// <summary>
        /// 平均执行时间
        /// 单位：秒
        /// </summary>
        public double RunTime { get; set; }
    }
}
