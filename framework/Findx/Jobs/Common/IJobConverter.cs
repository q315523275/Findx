namespace Findx.Jobs.Common;

/// <summary>
///     Job转换器
/// </summary>
public interface IJobConverter
{
    /// <summary>
    ///     转换为执行信息
    /// </summary>
    /// <param name="jobInfo"></param>
    /// <returns></returns>
    JobExecuteInfo ToExecuteInfo(JobInfo jobInfo);
}