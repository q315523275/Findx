using System;

namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 定时任务
    /// </summary>
    public partial class SysTimers
    {

        /// <summary>
        /// 定时器id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 执行任务的class的类名（实现了TimerTaskRunner接口的类的全称）
        /// </summary>
        public string ActionClass { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long? CreateUser { get; set; }

        /// <summary>
        /// 定时任务表达式
        /// </summary>
        public string Cron { get; set; } = string.Empty;

        /// <summary>
        /// 状态（字典 1运行  2停止）
        /// </summary>
        public sbyte? JobStatus { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// 任务名称
        /// </summary>
        public string TimerName { get; set; } = string.Empty;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        public long? UpdateUser { get; set; }

    }

}
