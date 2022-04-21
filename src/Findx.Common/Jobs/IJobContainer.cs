using System;
namespace Findx.Jobs
{
	/// <summary>
    /// 定义一个作业容器
    /// </summary>
	public interface IJobContainer
	{
		/// <summary>
        /// 默认调度器
        /// </summary>
		IJobScheduler Default { get; }

		/// <summary>
		/// 创建作业调度器
		/// </summary>
		/// <returns></returns>
		IJobScheduler Create();
	}
}

