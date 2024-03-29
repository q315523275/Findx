﻿using System.Diagnostics.Metrics;

namespace Findx.Metrics;

/// <summary>
///     诊断指标
/// </summary>
public class DiagnosticsMetrics : IMetricsClient
{
    private static readonly AssemblyName AssemblyName = typeof(DiagnosticsMetrics).Assembly.GetName();
    private static readonly string MeterName = AssemblyName.Name;
    private readonly ConcurrentDictionary<string, Counter<long>> _counters = new();
    private readonly ConcurrentDictionary<string, GaugeInfo> _gauges = new();

    private readonly Meter _meterInstance;
    private readonly string _prefix;
    private readonly ConcurrentDictionary<string, Histogram<double>> _timers = new();

    /// <summary>
    ///     Ctor
    /// </summary>
    public DiagnosticsMetrics()
    {
        _prefix = "";
        _meterInstance = new Meter(MeterName, AssemblyName.Version.ToString());
    }

    /// <summary>
    ///     计数器
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void Counter(string name, long value = 1)
    {
        var counter = _counters.GetOrAdd(_prefix + name, _meterInstance.CreateCounter<long>(name));
        counter.Add(value);
    }

    /// <summary>
    ///     测量
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void Gauge(string name, double value)
    {
        var gauge = _gauges.GetOrAdd(_prefix + name, new GaugeInfo(_meterInstance, name));
        gauge.Value = value;
    }

    /// <summary>
    ///     计时器
    /// </summary>
    /// <param name="name"></param>
    /// <param name="milliseconds"></param>
    public void Timer(string name, long milliseconds)
    {
        var timer = _timers.GetOrAdd(_prefix + name, _meterInstance.CreateHistogram<double>(name, "ms"));
        timer.Record(milliseconds);
    }

    /// <summary>
    /// </summary>
    public void Dispose()
    {
        _meterInstance.Dispose();
    }

    /// <summary>
    ///     测量信息
    /// </summary>
    private class GaugeInfo
    {
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="name"></param>
        public GaugeInfo(Meter meter, string name)
        {
            Gauge = meter.CreateObservableGauge(name, () => Value);
        }

        /// <summary>
        ///     测量仪表
        /// </summary>
        public ObservableGauge<double> Gauge { get; }

        /// <summary>
        ///     结果值
        /// </summary>
        public double Value { get; set; }
    }
}