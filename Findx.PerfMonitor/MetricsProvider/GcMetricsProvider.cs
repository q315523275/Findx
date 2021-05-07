using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;

namespace Findx.PerfMonitor.MetricsProvider
{
    public class GcMetricsProvider : IMetricsProvider
    {
        internal const string GcCountName = "clr.gc.count";
        internal const string GcTimeName = "clr.gc.time";
        internal const string GcGen0SizeName = "clr.gc.gen0size";
        internal const string GcGen1SizeName = "clr.gc.gen1size";
        internal const string GcGen2SizeName = "clr.gc.gen2size";
        internal const string GcGen3SizeName = "clr.gc.gen3size";

        private readonly bool _collectGcCount;
        private readonly bool _collectGcTime;
        private readonly bool _collectGcGen0Size;
        private readonly bool _collectGcGen1Size;
        private readonly bool _collectGcGen2Size;
        private readonly bool _collectGcGen3Size;

        private readonly GcEventListener _eventListener;
        private readonly object _lock = new object();

        private volatile bool _isMetricAlreadyCaptured;
        private uint _gcCount;
        private ulong _gen0Size;
        private ulong _gen1Size;
        private ulong _gen2Size;
        private ulong _gen3Size;
        private long _gcTimeInTicks;

        public string DbgName => throw new NotImplementedException();

        public IDictionary<string, object> GetSamples()
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// GC状态事件侦听器
        /// </summary>
        private class GcEventListener : EventListener
        {
            private static readonly int keywordGC = 1;
            private readonly GcMetricsProvider _gcMetricsProvider;
            private readonly ILogger<GcEventListener> _logger;
            private long _gcStartTime;

            public GcEventListener(GcMetricsProvider gcMetricsProvider, ILogger<GcEventListener> logger)
            {
                _gcMetricsProvider = gcMetricsProvider ?? throw new Exception("gcMetricsProvider is null");

                _logger = logger;
                _logger.LogTrace("初始化GC事件监听");
            }

            private EventSource _eventSourceDotNet;

            protected override void OnEventSourceCreated(EventSource eventSource)
            {
                try
                {
                    if (!eventSource.Name.Equals("Microsoft-Windows-DotNETRuntime"))
                        return;

                    EnableEvents(eventSource, EventLevel.Informational, (EventKeywords)keywordGC);
                    _eventSourceDotNet = eventSource;
                    _logger.LogTrace("Microsoft-Windows-DotNETRuntime enabled");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "EnableEvents failed - no GC metrics will be collected");
                }
            }

            protected override void OnEventWritten(EventWrittenEventArgs eventData)
            {
                // Collect heap sizes
                if (eventData.EventName.Contains("GCHeapStats_V1"))
                {
                    _logger.LogTrace("OnEventWritten with GCHeapStats_V1");

                    SetValue("GenerationSize0", ref _gcMetricsProvider._gen0Size);
                    SetValue("GenerationSize1", ref _gcMetricsProvider._gen1Size);
                    SetValue("GenerationSize2", ref _gcMetricsProvider._gen2Size);
                    SetValue("GenerationSize3", ref _gcMetricsProvider._gen3Size);

                    if (!_gcMetricsProvider._isMetricAlreadyCaptured)
                    {
                        lock (_gcMetricsProvider._lock)
                            _gcMetricsProvider._isMetricAlreadyCaptured = true;
                    }
                }

                if (eventData.EventName.Contains("GCHeapStats_V2"))
                {
                    _logger.LogTrace("OnEventWritten with GCHeapStats_V2");

                    SetValue("GenerationSize0", ref _gcMetricsProvider._gen0Size);
                    SetValue("GenerationSize1", ref _gcMetricsProvider._gen1Size);
                    SetValue("GenerationSize2", ref _gcMetricsProvider._gen2Size);
                    SetValue("GenerationSize3", ref _gcMetricsProvider._gen3Size);

                    if (!_gcMetricsProvider._isMetricAlreadyCaptured)
                    {
                        lock (_gcMetricsProvider._lock)
                            _gcMetricsProvider._isMetricAlreadyCaptured = true;
                    }
                }

                if (eventData.EventName.Contains("GCStart"))
                    Interlocked.Exchange(ref _gcStartTime, DateTime.UtcNow.Ticks);

                // Collect GC count and time
                if (eventData.EventName.Contains("GCEnd"))
                {
                    if (!_gcMetricsProvider._isMetricAlreadyCaptured)
                    {
                        lock (_gcMetricsProvider._lock)
                            _gcMetricsProvider._isMetricAlreadyCaptured = true;
                    }

                    _logger.LogTrace("OnEventWritten with GCEnd");

                    var durationInTicks = DateTime.UtcNow.Ticks - Interlocked.Read(ref _gcStartTime);
                    Interlocked.Exchange(ref _gcMetricsProvider._gcTimeInTicks, Interlocked.Read(ref _gcMetricsProvider._gcTimeInTicks) + durationInTicks);

                    var indexOfCount = IndexOf("Count");
                    if (indexOfCount < 0)
                        return;

                    var gcCount = eventData.Payload[indexOfCount];

                    if (!(gcCount is uint gcCountInt))
                        return;

                    _gcMetricsProvider._gcCount = gcCountInt;
                }

                void SetValue(string name, ref ulong value)
                {
                    var gen0SizeIndex = IndexOf(name);
                    if (gen0SizeIndex < 0)
                        return;

                    var gen0Size = eventData.Payload[gen0SizeIndex];
                    if (gen0Size is ulong gen0SizeLong)
                        value = gen0SizeLong;
                }

                int IndexOf(string name)
                {
                    return eventData.PayloadNames.IndexOf(name);
                }
            }

            public override void Dispose()
            {
                try
                {
                    if (_eventSourceDotNet != null)
                    {
                        _logger.LogTrace("disposing {classname}", nameof(GcEventListener));
                        DisableEvents(_eventSourceDotNet);
                        _eventSourceDotNet = null;
                        // calling _eventSourceDotNet.Dispose makes it impossible to re-enable the eventsource, so if we call _eventSourceDotNet.Dispose()
                        // all tests will fail after Dispose()
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Disposing {classname} failed", nameof(GcEventListener));
                }
            }
        }
    }
}
