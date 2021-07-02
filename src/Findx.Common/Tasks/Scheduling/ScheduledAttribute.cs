using System;

namespace Findx.Tasks.Scheduling
{
    /// <summary>
    /// 任务参数属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ScheduledAttribute : Attribute
    {
        /// <summary>
        /// Cron表达式
        /// </summary>
        public string Cron { set; get; }

        /// <summary>
        /// 任务执行间隔时间
        /// 单位：秒
        /// 以上一次方法执行完开始起算
        /// </summary>
        public double FixedDelay { get; set; }

        /// <summary>
        /// 任务名
        /// </summary>
        public string Name { get; set; }
    }
}
