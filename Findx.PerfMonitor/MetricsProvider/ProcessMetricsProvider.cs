using Findx.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Findx.PerfMonitor.MetricsProvider
{
    /// <summary>
    /// 当前进程统计提供器
    /// </summary>
    public class ProcessMetricsProvider : IMetricsProvider
    {
        internal const string ProcessCpuTotalPct = "system.process.cpu.total";
        internal const string ProcessVirtualMemory = "system.process.memory.virtual";
        internal const string ProcessWorkingSetMemory = "system.process.memory.working";

        public string DbgName => throw new NotImplementedException();

        private readonly ILogger<ProcessMetricsProvider> _logger;
        private readonly bool _collectProcessVirtualMemory;
        private readonly bool _collectProcessWorkingSetMemory;

        private TimeSpan _lastCurrentProcessCpuTime;
        private DateTime _lastTimeWindowStart;
        private Version _processAssemblyVersion;

        public ProcessMetricsProvider(ILogger<ProcessMetricsProvider> logger)
        {
            _logger = logger;
            _collectProcessVirtualMemory = true;
            _collectProcessWorkingSetMemory = true;

            try
            {
                _lastTimeWindowStart = DateTime.UtcNow;
                _lastCurrentProcessCpuTime = Process.GetCurrentProcess().TotalProcessorTime;
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Failed reading Process");
            }
        }

        public IDictionary<string, object> GetSamples()
        {
            #region CPU占用率

            var timeWindowStart = DateTime.UtcNow;
            var process = Process.GetCurrentProcess();
            var cpuUsage = process.TotalProcessorTime;
            var timeWindowEnd = DateTime.UtcNow;

            Dictionary<string, object> retVal = new Dictionary<string, object>();

            if (_processAssemblyVersion == null)
                _processAssemblyVersion = typeof(Process).Assembly.GetName().Version;

            double cpuUsedMs;

            if (Common.IsOsx && _processAssemblyVersion < new Version(4, 3, 0))
                cpuUsedMs = (cpuUsage - _lastCurrentProcessCpuTime).TotalMilliseconds / 100;
            else
                cpuUsedMs = (cpuUsage - _lastCurrentProcessCpuTime).TotalMilliseconds;

            var totalMsPassed = (timeWindowEnd - _lastTimeWindowStart).TotalMilliseconds;

            double cpuUsageTotal;

            if (totalMsPassed != 0)
                cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            else
                cpuUsageTotal = 0;

            _lastTimeWindowStart = timeWindowStart;
            _lastCurrentProcessCpuTime = cpuUsage;

            retVal.Add(ProcessCpuTotalPct, cpuUsageTotal);

            #endregion

            #region 虚拟内存、物理内存占用

            var virtualMemory = process.VirtualMemorySize64;
            var workingSet = process.WorkingSet64;

            if (_collectProcessVirtualMemory)
            {
                if (virtualMemory != 0)
                    retVal.Add(ProcessVirtualMemory, virtualMemory);
            }

            if (_collectProcessWorkingSetMemory)
            {
                if (workingSet != 0)
                    retVal.Add(ProcessWorkingSetMemory, workingSet);
            }

            #endregion

            return retVal;
        }
    }
}
