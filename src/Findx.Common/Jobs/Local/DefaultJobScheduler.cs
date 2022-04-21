using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.Extensions;
using Findx.Serialization;

namespace Findx.Jobs.Local
{
    public class DefaultJobScheduler : IJobScheduler
    {
        private readonly IJobStorage _storage;

        private readonly IJobDispatcher _dispatcher;

        private readonly IJsonSerializer _serializer;

        public DefaultJobScheduler(IJobStorage storage, IJobDispatcher dispatcher, IJsonSerializer serializer)
        {
            _storage = storage;
            _dispatcher = dispatcher;
            _serializer = serializer;
        }

        public async Task<long> EnqueueAsync<TJob>(object jobArgs, TimeSpan? delay = null) where TJob : IJob
        {
            var jobType = typeof(TJob);
            var jobDetail = CreateJobDetail(jobType, jobArgs);
            var nextRunTime = DateTimeOffset.UtcNow.LocalDateTime;

            jobDetail.IsSingle = true;
            jobDetail.NextRunTime = delay.HasValue ? nextRunTime.Add(delay.Value) : nextRunTime;

            await _storage.InsertAsync(jobDetail);
            return jobDetail.Id;
        }

        public async Task<long> EnqueueAsync<TJob>(object jobArgs, DateTime? dateTime = null) where TJob : IJob
        {
            var jobType = typeof(TJob);
            var jobDetail = CreateJobDetail(jobType, jobArgs);
            jobDetail.IsSingle = true;
            jobDetail.NextRunTime = dateTime ?? DateTimeOffset.UtcNow.LocalDateTime;

            await _storage.InsertAsync(jobDetail);
            return jobDetail.Id;
        }

        public async Task<long> ScheduleAsync<TJob>(object jobArgs, TimeSpan delay) where TJob : IJob
        {
            var jobType = typeof(TJob);
            var jobDetail = CreateJobDetail(jobType, jobArgs);
            jobDetail.IsSingle = false;
            jobDetail.FixedDelay = delay.TotalSeconds;
            jobDetail.NextRunTime = DateTimeOffset.UtcNow.Add(delay).LocalDateTime;

            await _storage.InsertAsync(jobDetail);
            return jobDetail.Id;
        }

        public async Task<long> ScheduleAsync<TJob>(object jobArgs, string cronExpression) where TJob : IJob
        {
            var jobType = typeof(TJob);
            var jobDetail = CreateJobDetail(jobType, jobArgs);
            jobDetail.IsSingle = false;
            jobDetail.CronExpress = cronExpression;
            jobDetail.NextRunTime = Utils.Cron.GetNextOccurrence(cronExpression);

            await _storage.InsertAsync(jobDetail);
            return jobDetail.Id;
        }

        public async Task<long> ScheduleAsync(Type jobType)
        {
            var attribute = jobType.GetAttribute<JobAttribute>();

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

        private JobInfo CreateJobDetail(Type jobType, object jsonParam)
        {
            var detail = new JobInfo
            {
                CreateTime = DateTimeOffset.UtcNow.LocalDateTime,
                IsEnable = true,
                NextRunTime = DateTimeOffset.UtcNow.LocalDateTime,
                Id = Findx.Utils.SnowflakeId.Default().NextId(),
                JsonParam = _serializer.Serialize(jsonParam ?? new { }),
                Name = jobType.Name,
                FullName = jobType.FullName,
                TryCount = 0,
            };

            var attribute = jobType.GetAttribute<JobAttribute>();
            if (attribute != null)
            {
                detail.Name = attribute.Name;
                detail.Remark = attribute.Description;
            }

            return detail;
        }

        /// <summary>
        /// 启动调度
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            return _dispatcher.StartAsync(cancellationToken);
        }

        /// <summary>
        /// 停止调度
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            return _dispatcher.StopAsync(cancellationToken);
        }
    }
}

