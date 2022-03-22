using System;
namespace Findx.Module.Admin.Sys.DTO
{
	/// <summary>
    /// 定时任务请求入参
    /// </summary>
	public class SysTimerRequest
	{
		/// <summary>
        /// 任务编号
        /// </summary>
		public string Id { set; get; }

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
        /// Cron表达式
        /// 最小粒度
        /// 以上一次方法执行完开始起算
        /// </summary>
        public string CronExpress { get; set; }
    }
}

