using System.Threading;
using Findx.Data;
using Findx.Utilities;

namespace Findx.Jobs;

/// <summary>
///     定义一个工作详细信息
/// </summary>
public class JobInfo : EntityBase<long>
{
    /// <summary>
    ///     作业编号
    /// </summary>
    public override long Id { get; set; }

    /// <summary>
    ///     作业名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     作业Json参数
    /// </summary>
    public string Parameter { get; set; }

    /// <summary>
    ///     作业类全名
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    ///     是否启用
    /// </summary>
    public bool IsEnable { get; set; }

    /// <summary>
    ///     执行中
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    ///     执行次数
    /// </summary>
    public int TryCount
    {
        get => _tryCount;
        set => _tryCount = value;
    }

    /// <summary>
    ///     执行次数 backing field（Increment 内用 Interlocked 原子自增）
    /// </summary>
    private int _tryCount;

    /// <summary>
    ///     是否单次任务
    /// </summary>
    public bool IsSingle { get; set; }

    /// <summary>
    ///     Cron表达式
    ///     最小粒度
    ///     以上一次方法执行完开始起算
    /// </summary>
    public string CronExpress { get; set; }

    /// <summary>
    ///     任务执行间隔时间
    ///     单位：秒
    ///     以上一次方法执行完开始起算
    /// </summary>
    public double FixedDelay { get; set; }

    /// <summary>
    ///     最后一次执行时间
    /// </summary>
    public DateTimeOffset? LastRunTime { get; set; }

    /// <summary>
    ///     下次执行时间
    /// </summary>
    public DateTimeOffset? NextRunTime { get; set; }

    /// <summary>
    ///     备注
    /// </summary>
    public string Comments { get; set; }

    /// <summary>
    ///     任务创建时间
    /// </summary>
    public DateTimeOffset CreatedTime { get; set; }

    /// <summary>
    ///     任务计数
    /// </summary>
    public void Increment(DateTimeOffset dateTimeOffset)
    {
        //  计数原子自增；注意 IsRunning/LastRunTime/NextRunTime 多字段切换不是原子的，
        //  调用方应在存储层锁内串行操作同一 JobInfo 实例（见 InMemoryJobStorage）
        Interlocked.Increment(ref _tryCount);
        IsRunning = false;
        LastRunTime = NextRunTime;
        if (IsSingle)
        {
            NextRunTime = null;
        }
        else if (FixedDelay > 0)
        {
            NextRunTime = dateTimeOffset.AddSeconds(FixedDelay);
        }
        else if (!string.IsNullOrWhiteSpace(CronExpress))
        {
            NextRunTime = CronUtility.GetNextOccurrence(CronExpress, dateTimeOffset);
        }
    }
}