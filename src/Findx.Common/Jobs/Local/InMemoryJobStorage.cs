using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Findx.Extensions;

namespace Findx.Jobs.Local
{
    public class InMemoryJobStorage : IJobStorage
    {
        // private readonly IDictionary<long, JobInfo> _jobs;

        private readonly List<JobInfo> _jobs;

        public InMemoryJobStorage()
        {
            // 业务特性不需要使用线程安全字典
            // _jobs = new Dictionary<long, JobInfo>();
            _jobs = new List<JobInfo>();
        }

        public Task DeleteAsync(long jobId)
        {
            //_jobs.Remove(jobId);
            _jobs.RemoveAll(x => x.Id == jobId);
            return Task.CompletedTask;
        }

        public Task<JobInfo> FindAsync(long jobId)
        {
            // var jobDetail = _jobs.GetOrDefault(jobId);
            // return Task.FromResult(jobDetail);

            return Task.FromResult(_jobs.FirstOrDefault(x => x.Id == jobId));
        }

        public Task<IEnumerable<JobInfo>> GetJobsAsync()
        {
            return Task.FromResult(_jobs.AsEnumerable());
        }

        public Task<IEnumerable<JobInfo>> GetShouldRunJobsAsync(int maxResultCount)
        {
            var referenceTime = DateTimeOffset.UtcNow.LocalDateTime;
            //var tasksThatShouldRun = _jobs.Values
            //                              .Where(t => t.ShouldRun(referenceTime))
            //                              .OrderBy(t => t.TryCount)
            //                              .ThenBy(t => t.NextRunTime)
            //                              .Take(maxResultCount);
            //return Task.FromResult(tasksThatShouldRun);

            return Task.FromResult(_jobs.Where(t => t.ShouldRun(referenceTime)));
        }

        public Task InsertAsync(JobInfo detail)
        {
            //_jobs.TryAdd(detail.Id, detail);

            _jobs.TryAdd(detail);

            return Task.CompletedTask;
        }

        public Task UpdateAsync(JobInfo detail)
        {
            //_jobs[detail.Id] = detail;

            _jobs.ReplaceOne(x => x.Id == detail.Id, detail);

            return Task.CompletedTask;
        }
    }
}

