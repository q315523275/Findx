using System.Threading.Tasks;
using Findx.Common;
using Findx.Data;
using Findx.Extensions;
using Findx.Jobs.Client;
using Findx.Jobs.Storage;
using Findx.Serialization;
using Findx.Utilities;

namespace Findx.Jobs.Common;

/// <summary>
///     默认工作调度
/// </summary>
public class SimpleJobScheduler : IJobScheduler
{
    private readonly IJsonSerializer _serializer;
    private readonly IJobStorage _storage;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="storage"></param>
    /// <param name="serializer"></param>
    public SimpleJobScheduler(IJobStorage storage, IJsonSerializer serializer)
    {
        _storage = storage;
        _serializer = serializer;
    }

    /// <summary>
    ///     添加一次性任务
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="parameter"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TJob"></typeparam>
    /// <returns></returns>
    public async Task<long> EnqueueAsync<TJob>(TimeSpan? delay = null, IDictionary<string, string> parameter = null, CancellationToken cancellationToken = default) where TJob : IJob
    {
        var jobType = typeof(TJob);
        var jobDetail = CreateJobDetail(jobType, parameter);
        //  统一使用 UTC 瞬时值，避免 DateTime Kind（Local/Unspecified）混用导致跨时区偏移
        var nextRunTime = DateTimeOffset.UtcNow;

        jobDetail.IsSingle = true;
        jobDetail.NextRunTime = delay.HasValue ? nextRunTime.Add(delay.Value) : nextRunTime;

        await _storage.InsertAsync(jobDetail, cancellationToken);
        return jobDetail.Id;
    }

    /// <summary>
    ///     添加一次性任务
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="parameter"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TJob"></typeparam>
    /// <returns></returns>
    public async Task<long> EnqueueAsync<TJob>(DateTime? dateTime = null, IDictionary<string, string> parameter = null, CancellationToken cancellationToken = default) where TJob : IJob
    {
        var jobType = typeof(TJob);
        var jobDetail = CreateJobDetail(jobType, parameter);
        jobDetail.IsSingle = true;
        //  调用方传入的 DateTime 保持原有隐式转换语义（按 Kind 解释为瞬时值），空值回退当前 UTC 时间
        jobDetail.NextRunTime = dateTime.HasValue ? dateTime.Value : DateTimeOffset.UtcNow;

        await _storage.InsertAsync(jobDetail, cancellationToken);
        return jobDetail.Id;
    }

    /// <summary>
    ///     添加循环任务
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="parameter"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TJob"></typeparam>
    /// <returns></returns>
    public async Task<long> ScheduleAsync<TJob>(TimeSpan delay, IDictionary<string, string> parameter = null, CancellationToken cancellationToken = default) where TJob : IJob
    {
        var jobType = typeof(TJob);
        var jobDetail = CreateJobDetail(jobType, parameter);
        jobDetail.IsSingle = false;
        jobDetail.FixedDelay = delay.TotalSeconds;
        jobDetail.NextRunTime = DateTimeOffset.UtcNow.Add(delay);

        await _storage.InsertAsync(jobDetail, cancellationToken);
        return jobDetail.Id;
    }

    /// <summary>
    ///     添加循环任务
    /// </summary>
    /// <param name="cronExpression"></param>
    /// <param name="parameter"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TJob"></typeparam>
    /// <returns></returns>
    public async Task<long> ScheduleAsync<TJob>(string cronExpression, IDictionary<string, string> parameter = null, CancellationToken cancellationToken = default) where TJob : IJob
    {
        var jobType = typeof(TJob);
        var jobDetail = CreateJobDetail(jobType, parameter);
        jobDetail.IsSingle = false;
        jobDetail.CronExpress = cronExpression;
        jobDetail.NextRunTime = CronUtility.GetNextOccurrence(cronExpression);

        await _storage.InsertAsync(jobDetail, cancellationToken);
        return jobDetail.Id;
    }

    /// <summary>
    ///     添加循环任务,Type必须带属性注解
    /// </summary>
    /// <param name="jobType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<long> ScheduleAsync(Type jobType, CancellationToken cancellationToken = default)
    {
        var attribute = GetJobAttribute(jobType);

        Check.NotNull(attribute, nameof(attribute));

        var jobDetail = CreateJobDetail(jobType, null);

        if (!attribute.Interval.IsNullOrWhiteSpace())
        {
            var span = TimeSpanUtility.ToTimeSpan(attribute.Interval);
            jobDetail.IsSingle = false;
            jobDetail.FixedDelay = span.TotalSeconds;
            jobDetail.NextRunTime = DateTimeOffset.UtcNow.Add(span);
        }

        if (!attribute.Cron.IsNullOrWhiteSpace())
        {
            jobDetail.IsSingle = false;
            jobDetail.CronExpress = attribute.Cron;
            jobDetail.NextRunTime = CronUtility.GetNextOccurrence(attribute.Cron);
        }

        await _storage.InsertAsync(jobDetail, cancellationToken);
        return jobDetail.Id;
    }

    /// <summary>
    ///     暂停任务
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task PauseJob(long id, CancellationToken cancellationToken = default)
    {
        var jobInfo = await _storage.FindAsync(id, cancellationToken);
        if (jobInfo is { IsEnable: true })
        {
            jobInfo.IsEnable = false;
            await _storage.UpdateAsync(jobInfo, cancellationToken);
        }
    }

    /// <summary>
    ///     恢复暂停任务
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ResumeJob(long id, CancellationToken cancellationToken = default)
    {
        var jobInfo = await _storage.FindAsync(id, cancellationToken);
        if (jobInfo is { IsEnable: false })
        {
            jobInfo.IsEnable = true;
            await _storage.UpdateAsync(jobInfo, cancellationToken);
        }
    }

    /// <summary>
    ///     删除任务
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task RemoveJob(long id, CancellationToken cancellationToken = default)
    {
        await _storage.DeleteAsync(id, cancellationToken);
    }

    /// <summary>
    ///     创建任务信息
    /// </summary>
    /// <param name="jobType"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    private JobInfo CreateJobDetail(Type jobType, IDictionary<string, string> parameter)
    {
        var detail = new JobInfo
        {
            CreatedTime = DateTimeOffset.UtcNow,
            IsEnable = true,
            NextRunTime = DateTimeOffset.UtcNow,
            Id = SnowflakeIdUtility.Default().NextId(),
            Parameter = _serializer.Serialize(parameter ?? new Dictionary<string, string>()),
            Name = jobType.Name,
            FullName = jobType.FullName,
            TryCount = 0
        };

        var attribute = GetJobAttribute(jobType);
        if (attribute != null)
        {
            detail.Name = attribute.Name;
            detail.Comments = attribute.Description;
        }

        return detail;
    }

    /// <summary>
    ///     获取作业类型上的特性注解（带全局缓存）
    ///     <para>注意：ConcurrentDictionary 不允许写入 null 值，未标注特性的类型只返回 null、不入缓存</para>
    /// </summary>
    /// <param name="jobType"></param>
    /// <returns></returns>
    private static JobAttribute GetJobAttribute(Type jobType)
    {
        var cache = (ConcurrentDictionary<Type, JobAttribute>)SingletonDictionary<Type, JobAttribute>.Instance;
        if (cache.TryGetValue(jobType, out var cached))
            return cached;

        var attribute = jobType.GetAttribute<JobAttribute>();
        if (attribute != null)
            cache.TryAdd(jobType, attribute);

        return attribute;
    }
}