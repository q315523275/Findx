﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.Data;
using Findx.Extensions;
using Findx.Serialization;

namespace Findx.Jobs.Local
{
    /// <summary>
    /// 默认工作调度
    /// </summary>
    public class DefaultJobScheduler : IJobScheduler
    {
        private readonly IJobStorage _storage;

        private readonly IJsonSerializer _serializer;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="serializer"></param>
        public DefaultJobScheduler(IJobStorage storage, IJsonSerializer serializer)
        {
            _storage = storage;
            _serializer = serializer;
        }

        /// <summary>
        /// 添加一次性任务
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="parameter"></param>
        /// <typeparam name="TJob"></typeparam>
        /// <returns></returns>
        public async Task<long> EnqueueAsync<TJob>(TimeSpan? delay = null, IDictionary<string, string> parameter = null) where TJob : IJob
        {
            var jobType = typeof(TJob);
            var jobDetail = CreateJobDetail(jobType, parameter);
            var nextRunTime = DateTimeOffset.UtcNow.LocalDateTime;

            jobDetail.IsSingle = true;
            jobDetail.NextRunTime = delay.HasValue ? nextRunTime.Add(delay.Value) : nextRunTime;

            await _storage.InsertAsync(jobDetail);
            return jobDetail.Id;
        }

        /// <summary>
        /// 添加一次性任务
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="parameter"></param>
        /// <typeparam name="TJob"></typeparam>
        /// <returns></returns>
        public async Task<long> EnqueueAsync<TJob>(DateTime? dateTime = null, IDictionary<string, string> parameter = null) where TJob : IJob
        {
            var jobType = typeof(TJob);
            var jobDetail = CreateJobDetail(jobType, parameter);
            jobDetail.IsSingle = true;
            jobDetail.NextRunTime = dateTime ?? DateTimeOffset.UtcNow.LocalDateTime;

            await _storage.InsertAsync(jobDetail);
            return jobDetail.Id;
        }

        /// <summary>
        /// 添加循环任务
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="parameter"></param>
        /// <typeparam name="TJob"></typeparam>
        /// <returns></returns>
        public async Task<long> ScheduleAsync<TJob>(TimeSpan delay, IDictionary<string, string> parameter = null) where TJob : IJob
        {
            var jobType = typeof(TJob);
            var jobDetail = CreateJobDetail(jobType, parameter);
            jobDetail.IsSingle = false;
            jobDetail.FixedDelay = delay.TotalSeconds;
            jobDetail.NextRunTime = DateTimeOffset.UtcNow.Add(delay).LocalDateTime;

            await _storage.InsertAsync(jobDetail);
            return jobDetail.Id;
        }

        /// <summary>
        /// 添加循环任务
        /// </summary>
        /// <param name="cronExpression"></param>
        /// <param name="parameter"></param>
        /// <typeparam name="TJob"></typeparam>
        /// <returns></returns>
        public async Task<long> ScheduleAsync<TJob>(string cronExpression, IDictionary<string, string> parameter = null) where TJob : IJob
        {
            var jobType = typeof(TJob);
            var jobDetail = CreateJobDetail(jobType, parameter);
            jobDetail.IsSingle = false;
            jobDetail.CronExpress = cronExpression;
            jobDetail.NextRunTime = Utils.Cron.GetNextOccurrence(cronExpression);

            await _storage.InsertAsync(jobDetail);
            return jobDetail.Id;
        }

        /// <summary>
        /// 添加循环任务,Type必须带属性注解
        /// </summary>
        /// <param name="jobType"></param>
        /// <returns></returns>
        public async Task<long> ScheduleAsync(Type jobType)
        {
            var attribute = SingletonDictionary<Type, JobAttribute>.Instance.GetOrAdd(jobType, () => jobType.GetAttribute<JobAttribute>());

            Check.NotNull(attribute, nameof(attribute));

            var jobDetail = CreateJobDetail(jobType, null);

            if (!attribute.Interval.IsNullOrWhiteSpace())
            {
                var span = Findx.Utils.Time.ToTimeSpan(attribute.Interval);
                jobDetail.IsSingle = false;
                jobDetail.FixedDelay = span.TotalSeconds;
                jobDetail.NextRunTime = DateTimeOffset.UtcNow.Add(span).LocalDateTime;
            }

            if (!attribute.Cron.IsNullOrWhiteSpace())
            {
                jobDetail.IsSingle = false;
                jobDetail.CronExpress = attribute.Cron;
                jobDetail.NextRunTime = Utils.Cron.GetNextOccurrence(attribute.Cron);
            }

            await _storage.InsertAsync(jobDetail);
            return jobDetail.Id;
        }

        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task PauseJob(long id)
        {
            var jobInfo = await _storage.FindAsync(id);
            if (jobInfo != null && jobInfo.IsEnable)
            {
                jobInfo.IsEnable = false;
                await _storage.UpdateAsync(jobInfo);
            }
        }

        /// <summary>
        /// 恢复暂停任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task ResumeJob(long id)
        {
            var jobInfo = await _storage.FindAsync(id);
            if (jobInfo != null && !jobInfo.IsEnable)
            {
                jobInfo.IsEnable = true;
                await _storage.UpdateAsync(jobInfo);
            }
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task RemoveJob(long id)
        {
            await _storage.DeleteAsync(id);
        }

        /// <summary>
        /// 创建任务信息
        /// </summary>
        /// <param name="jobType"></param>
        /// <param name="jsonParam"></param>
        /// <returns></returns>
        private JobInfo CreateJobDetail(Type jobType, IDictionary<string, string> parameter)
        {
            var detail = new JobInfo
            {
                CreateTime = DateTimeOffset.UtcNow.LocalDateTime,
                IsEnable = true,
                NextRunTime = DateTimeOffset.UtcNow.LocalDateTime,
                Id = Findx.Utils.SnowflakeId.Default().NextId(),
                JsonParam = _serializer.Serialize(parameter ?? new Dictionary<string, string>()),
                Name = jobType.Name,
                FullName = jobType.FullName,
                TryCount = 0,
            };

            var attribute = SingletonDictionary<Type, JobAttribute>.Instance.GetOrAdd(jobType, () => jobType.GetAttribute<JobAttribute>());
            if (attribute != null)
            {
                detail.Name = attribute.Name;
                detail.Remark = attribute.Description;
            }

            return detail;
        }
    }
}

