using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Findx.Jobs
{
	/// <summary>
    /// 定义一个工作存储器
    /// </summary>
	public interface IJobStorage
	{
        /// <summary>
        /// 存储任务信息
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        Task InsertAsync(JobInfo detail);

        /// <summary>
        /// 删除任务信息
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        Task DeleteAsync(long jobId);

        /// <summary>
        /// 更新任务信息
        /// </summary>
        /// <param name="taskInfo"></param>
        /// <returns></returns>
        Task UpdateAsync(JobInfo detail);

        /// <summary>
        /// 查询任务信息
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        Task<JobInfo> FindAsync(long jobId);

        /// <summary>
        /// 查询可以执行任务列表
        /// </summary>
        /// <param name="maxResultCount"></param>
        /// <returns></returns>
        Task<IEnumerable<JobInfo>> GetShouldRunJobsAsync(int maxResultCount);

        /// <summary>
        /// 查询所有任务
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<JobInfo>> GetJobsAsync();
    }
}

