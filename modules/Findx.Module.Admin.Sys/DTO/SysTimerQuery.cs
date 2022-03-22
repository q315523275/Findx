using System;
using Findx.Data;

namespace Findx.Module.Admin.Sys.DTO
{
	/// <summary>
    /// 系统定时任务查询入参
    /// </summary>
	public class SysTimerQuery : PageBase
	{
		/// <summary>
        /// 任务名
        /// </summary>
		public string taskName { set; get; }

		/// <summary>
        /// 任务状态
        /// </summary>
		public int? jobStatus { set; get; }
	}
}

