using System.Collections.Generic;

namespace Findx.PerfMonitor
{
    public interface IMetricsProvider
    {
        string DbgName { get; }
        IDictionary<string, object> GetSamples();
    }
}
