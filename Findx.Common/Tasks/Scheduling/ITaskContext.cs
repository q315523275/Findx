using System;
using System.Collections.Generic;

namespace Findx.Tasks.Scheduling
{
    public interface ITaskContext
    {
        IServiceProvider ServiceProvider { get; }
        Guid TaskId { get; }
        IDictionary<string, string> Parameter { get; }
        int ShardIndex { get; }
        int ShardTotal { get; }
    }
}
