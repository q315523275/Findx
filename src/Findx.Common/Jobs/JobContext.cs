using System;
using System.Collections.Generic;

namespace Findx.Jobs
{
	public class JobContext: IJobContext
	{
        public JobContext(IServiceProvider serviceProvider, long jobId, long executionId, string fullName)
        {
            ServiceProvider = serviceProvider;
            JobId = jobId;
            ExecutionId = executionId;
            FullName = fullName;
        }

        public IServiceProvider ServiceProvider { get; }

        public long JobId { get; }

        public long ExecutionId { get; }

        public string FullName { get; }

        // 非必填参数
        public string JobName { get; set; }

        public IDictionary<string, string> Parameter { get; set; } = new Dictionary<string, string>();

        public int ShardIndex { get; set; }

        public int ShardTotal { get; set; }
    }
}

