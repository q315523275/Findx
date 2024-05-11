using Findx.Data;

namespace Findx.Jobs.Common;

/// <summary>
///     Job转换器
/// </summary>
public class JobConverter : IJobConverter
{
    private readonly IKeyGenerator<long> _keyGenerator;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="keyGenerator"></param>
    public JobConverter(IKeyGenerator<long> keyGenerator)
    {
        _keyGenerator = keyGenerator;
    }

    /// <summary>
    ///     转换
    /// </summary>
    /// <param name="jobInfo"></param>
    /// <returns></returns>
    public JobExecuteInfo ToExecuteInfo(JobInfo jobInfo)
    {
        return new JobExecuteInfo
        {
            Id = _keyGenerator.Create(), 
            Name = jobInfo.Name, 
            Parameter = jobInfo.Parameter, 
            FullName = jobInfo.FullName, 
            JobId = jobInfo.Id, 
            RunTime = jobInfo.NextRunTime
        };
    }
}