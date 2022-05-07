using System;

namespace Findx.Metrics
{
    /// <summary>
    /// 性能计数器
    /// </summary>
    public interface IMetricsClient : IDisposable
    {
        void Counter(string name, long value = 1);
        void Gauge(string name, double value);
        void Timer(string name, long milliseconds);
    }
}
