using Findx.Data;

namespace Findx.Jobs;

/// <summary>
///     定义一个工作详细信息
/// </summary>
public class JobExecuteInfo : EntityBase<long>
{
    /// <summary>
    ///     执行编号
    /// </summary>
    public override long Id { get; set; }

    /// <summary>
    ///     任务编号
    /// </summary>
    public long JobId { get; set; }
    
    /// <summary>
    ///     任务名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     执行参数
    /// </summary>
    public string Parameter { get; set; }

    /// <summary>
    ///     任务类全名
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    ///     执行时间
    /// </summary>
    public DateTimeOffset? RunTime { get; set; }
}