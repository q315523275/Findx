namespace Findx.PerfMonitor
{
    public interface IMetricsProvider
    {
        string DbgName { get; }

        // IEnumerable<MetricSample> GetSamples();
    }
}
