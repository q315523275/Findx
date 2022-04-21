using System;
using Microsoft.Extensions.Options;

namespace Findx.Jobs
{
	/// <summary>
    /// 作业参数
    /// </summary>
	public class JobOptions : IOptions<JobOptions>
	{
		/// <summary>
		/// 是否启用
		/// </summary>
		public bool Enabled { set; get; }

		/// <summary>
		/// 延迟时间
		/// <para>单位:毫秒</para>
		/// </summary>
		public int Delay { set; get; } = 800;

		/// <summary>
		/// 单次最大调度作业数
		/// 支持OnChange
		/// </summary>
		public int MaxFetchJobCount { set; get; } = 1000;

		/// <summary>
        /// option
        /// </summary>
        public JobOptions Value => this;
    }
}

