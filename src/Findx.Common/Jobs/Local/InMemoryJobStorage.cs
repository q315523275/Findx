using System.Threading.Tasks;
using Findx.Extensions;

namespace Findx.Jobs.Local
{
    public class InMemoryJobStorage : IJobStorage
    {
        private readonly List<JobInfo> _jobs;

        public InMemoryJobStorage()
        {
            // 字典存储也可以

            _jobs = new List<JobInfo>();
        }

        public Task DeleteAsync(long jobId)
        {
            _jobs.RemoveAll(x => x.Id == jobId);

            return Task.CompletedTask;
        }

        public Task<JobInfo> FindAsync(long jobId)
        {
            return Task.FromResult(_jobs.FirstOrDefault(x => x.Id == jobId));
        }

        public Task<IEnumerable<JobInfo>> GetJobsAsync()
        {
            return Task.FromResult(_jobs.AsEnumerable());
        }

        public Task<IEnumerable<JobInfo>> GetShouldRunJobsAsync(int maxResultCount)
        {
            var referenceTime = DateTimeOffset.UtcNow.LocalDateTime;
            var tasksThatShouldRun = _jobs.Where(t => t.ShouldRun(referenceTime))
                                          .OrderBy(t => t.TryCount)
                                          .ThenBy(t => t.NextRunTime)
                                          .Take(maxResultCount);
            return Task.FromResult(tasksThatShouldRun);
        }

        public Task InsertAsync(JobInfo detail)
        {
            _jobs.TryAdd(detail);

            return Task.CompletedTask;
        }

        public Task UpdateAsync(JobInfo detail)
        {

            _jobs.ReplaceOne(x => x.Id == detail.Id, detail);

            return Task.CompletedTask;
        }
    }
}

