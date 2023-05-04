namespace Findx.Metrics;

/// <summary>
///     性能计数器
/// </summary>
public interface IMetricsClient : IDisposable
{
    /// <summary>
    ///     计数器
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    void Counter(string name, long value = 1);

    /// <summary>
    ///     测量器
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    void Gauge(string name, double value);

    /// <summary>
    ///     计时器
    /// </summary>
    /// <param name="name"></param>
    /// <param name="milliseconds"></param>
    void Timer(string name, long milliseconds);
}