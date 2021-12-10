using Microsoft.Extensions.Options;

namespace Findx.Scheduling
{
    /// <summary>
    /// 调度器参数
    /// </summary>
    public class SchedulerOptions : IOptions<SchedulerOptions>
    {
        /// <summary>
        /// 调度间隔时间
        /// 单位：毫秒
        /// 支持OnChange
        /// </summary>
        public int ScheduleMillisecondsDelay { get; set; } = 500;
        /// <summary>
        /// 单次最大调度任务数
        /// 支持OnChange
        /// </summary>
        public int MaxJobFetchCount { set; get; } = 1000;
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { set; get; }

        public SchedulerOptions Value => this;

        public override string ToString()
        {
            return $"{ScheduleMillisecondsDelay}{MaxJobFetchCount}{Enabled}";
        }
    }
}
