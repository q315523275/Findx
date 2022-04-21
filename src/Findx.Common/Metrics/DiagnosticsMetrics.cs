using System;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace Findx.Metrics
{
    public class DiagnosticsMetrics : IMetrics
    {
        private readonly ConcurrentDictionary<string, Counter<long>> _counters = new();
        private readonly ConcurrentDictionary<string, GaugeInfo> _gauges = new();
        private readonly ConcurrentDictionary<string, Histogram<double>> _timers = new();
        private readonly Meter _meter;
        private readonly string _prefix;

        public DiagnosticsMetrics()
        {
            _meter = new Meter("Findx.Metrics", this.GetType().Assembly.GetName().Version.ToString());
        }

        public void Counter(string name, long value = 1)
        {
            var counter = _counters.GetOrAdd(_prefix + name, _meter.CreateCounter<long>(name));
            counter.Add(value);
        }

        public void Gauge(string name, double value)
        {
            var gauge = _gauges.GetOrAdd(_prefix + name, new GaugeInfo(_meter, name));
            gauge.Value = value;
        }

        public void Timer(string name, long milliseconds)
        {
            var timer = _timers.GetOrAdd(_prefix + name, _meter.CreateHistogram<double>(name, "ms"));
            timer.Record(milliseconds);
        }

        public void Dispose()
        {
            _meter.Dispose();
        }

        private class GaugeInfo
        {
            public GaugeInfo(Meter meter, string name)
            {
                Gauge = meter.CreateObservableGauge(name, () => Value);
            }

            public ObservableGauge<double> Gauge { get; }
            public double Value { get; set; }
        }
    }
}

