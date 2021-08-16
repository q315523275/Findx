using Microsoft.Extensions.Options;

namespace Findx.Tasks.Scheduling
{
    /// <summary>
    /// 调度器参数
    /// </summary>
    public class SchedulerOptions : IOptions<SchedulerOptions>
    {
        /// <summary>
        /// 调度间隔时间
        /// 单位：秒
        /// </summary>
        public int JobPollPeriod { get; set; } = 1;
        /// <summary>
        /// 单次最大调度任务数
        /// </summary>
        public int MaxJobFetchCount { set; get; } = 1000;
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { set; get; }

        public SchedulerOptions Value => this;

        public override string ToString()
        {
            return $"{JobPollPeriod}{MaxJobFetchCount}{Enable}";
        }
    }
}
