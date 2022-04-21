using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Findx.Extensions;

namespace Findx.Jobs.Local
{
    public class InMemoryJobStorage : IJobStorage
    {
        private readonly IDictionary<long, JobInfo> _jobs;

        public InMemoryJobStorage()
        {
            // 业务特性不需要使用线程安全字典
            _jobs = new Dictionary<long, JobInfo>();
        }

        public Task DeleteAsync(long jobId)
        {
            _jobs.Remove(jobId);
            return Task.CompletedTask;
        }

        public Task<JobInfo> FindAsync(long jobId)
        {
            var jobDetail = _jobs.GetOrDefault(jobId);

            return Task.FromResult(jobDetail);
        }

        public Task<IEnumerable<JobInfo>> GetJobsAsync()
        {
            return Task.FromResult(_jobs.Values.AsEnumerable());
        }

        public Task<IEnumerable<JobInfo>> GetShouldRunJobsAsync(int maxResultCount)
        {
            var referenceTime = DateTimeOffset.UtcNow.LocalDateTime;
            var tasksThatShouldRun = _jobs.Values
                                          .Where(t => t.ShouldRun(referenceTime))
                                          .OrderBy(t => t.TryCount)
                                          .ThenBy(t => t.NextRunTime)
                                          .Take(maxResultCount);
            return Task.FromResult(tasksThatShouldRun);
        }

        public Task InsertAsync(JobInfo detail)
        {
            _jobs.TryAdd(detail.Id, detail);

            return Task.CompletedTask;
        }

        public Task UpdateAsync(JobInfo detail)
        {
            _jobs[detail.Id] = detail;
            return Task.CompletedTask;
        }
    }
}

