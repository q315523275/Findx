using Findx.PerfMonitor.Windows;
using Findx.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace Findx.PerfMonitor.MetricsProvider
{
    /// <summary>
    /// 获取服务器总内容和可用内存大小
    /// 目前只支持Windows和Linux
    /// </summary>
    public class FreeAndTotalMemoryProvider : IMetricsProvider
    {
        internal const string FreeMemory = "system.memory.free";
        internal const string TotalMemory = "system.memory.total";

        public string DbgName => "total and free memory";

        public int ConsecutiveNumberOfFailedReads { get; set; }
        private readonly bool _collectFreeMemory;
        private readonly bool _collectTotalMemory;

        public FreeAndTotalMemoryProvider()
        {
            _collectFreeMemory = true;
            _collectTotalMemory = true;
        }

        public IDictionary<string, object> GetSamples()
        {
            if (Common.IsWindows)
            {
                var (success, totalMemory, freeMemory) = GlobalMemoryStatus.GetTotalPhysAndAvailPhys();

                if (!success || totalMemory == 0 || freeMemory == 0)
                    return null;

                var retVal = new Dictionary<string, object>();

                if (_collectFreeMemory)
                    retVal.Add(FreeMemory, freeMemory);

                if (_collectTotalMemory)
                    retVal.Add(TotalMemory, totalMemory);

                return retVal;
            }

            if (Common.IsLinux)
            {
                var retVal = new Dictionary<string, object>();

                using (var sr = new StreamReader("/proc/meminfo"))
                {
                    var hasMemFree = false;
                    var hasMemTotal = false;

                    var line = sr.ReadLine();

                    while (line != null || retVal.Count != 2)
                    {
                        //See: https://github.com/elastic/beats/issues/4202
                        if (line != null && line.Contains("MemAvailable:") && _collectFreeMemory)
                        {
                            var (suc, res) = GetEntry(line, "MemAvailable:");
                            if (suc) retVal.Add(FreeMemory, res);
                            hasMemFree = true;
                        }
                        if (line != null && line.Contains("MemTotal:") && _collectTotalMemory)
                        {
                            var (suc, res) = GetEntry(line, "MemTotal:");
                            if (suc) retVal.Add(TotalMemory, res);
                            hasMemTotal = true;
                        }

                        if ((hasMemFree || !_collectFreeMemory) && (hasMemTotal || !_collectTotalMemory))
                            break;

                        line = sr.ReadLine();
                    }
                }

                ConsecutiveNumberOfFailedReads = 0;

                return retVal;
            }

            (bool, ulong) GetEntry(string line, string name)
            {
                var nameIndex = line.IndexOf(name, StringComparison.Ordinal);
                if (nameIndex < 0)
                    return (false, 0);

                var values = line.Substring(line.IndexOf(name, StringComparison.Ordinal) + name.Length);

                if (string.IsNullOrWhiteSpace(values)) return (false, 0);

                var items = values.Trim().Split(' ');

                switch (items.Length)
                {
                    case 1 when ulong.TryParse(items[0], out var res): return (true, res);
                    case 2 when items[1].ToLowerInvariant() == "kb" && ulong.TryParse(items[0], out var res): return (true, res * 1024);
                    default: return (false, 0);
                }
            }

            return null;

        }
    }
}
