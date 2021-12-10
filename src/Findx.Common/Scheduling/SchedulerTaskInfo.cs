using System;

namespace Findx.Scheduling
{
    public class SchedulerTaskInfo
    {
        public Guid Id { get; set; }
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
        /// 执行中
        /// </summary>
        public bool IsRuning { get; set; }

        /// <summary>
        /// 执行次数
        /// </summary>
        public int TryCount { get; set; }

        /// <summary>
        /// 是否单次任务
        /// </summary>
        public bool IsSingle { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable { get; set; }

        /// <summary>
        /// Cron表达式
        /// 最小粒度
        /// 以上一次方法执行完开始起算
        /// </summary>
        public string CronExpress { get; set; }

        /// <summary>
        /// 任务执行间隔时间
        /// 单位：秒
        /// 以上一次方法执行完开始起算
        /// </summary>
        public double FixedDelay { get; set; }

        /// <summary>
        /// 任务创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后一次执行时间
        /// </summary>
        public DateTime? LastRunTime { get; set; }

        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTime? NextRunTime { get; set; }

        /// <summary>
        /// 延时执行时间
        /// 单位：秒
        /// </summary>
        public long DelayRunTime { get; set; }

        /// <summary>
        /// 平均执行时间
        /// 单位：秒
        /// </summary>
        public long RunTime { get; set; }

        /// <summary>
        /// 任务计数
        /// </summary>
        public void Increment()
        {
            TryCount++;
            IsRuning = false;
            LastRunTime = NextRunTime.Value;
            if (IsSingle)
            {
                NextRunTime = null;
            }
            else if (FixedDelay > 0)
            {
                NextRunTime = DateTimeOffset.UtcNow.LocalDateTime.Add(TimeSpan.FromSeconds(FixedDelay));
            }
            else if (!string.IsNullOrWhiteSpace(CronExpress))
            {
                NextRunTime = Utils.Cron.GetNextOccurrence(CronExpress);
            }
        }

        public bool ShouldRun(DateTime currentTime)
        {
            return IsEnable && !IsRuning && NextRunTime.HasValue && NextRunTime <= currentTime && LastRunTime != NextRunTime;
        }
    }
}
