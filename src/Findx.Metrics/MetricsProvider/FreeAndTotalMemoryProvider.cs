using Findx.PerfMonitor.Windows;

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

                var dic = Common.ReadInfo("/proc/meminfo");
                if (dic != null)
                {
                    if (dic.TryGetValue("MemTotal", out var str))
                        retVal.Add(TotalMemory, str.RemovePostFix(" kB").To<int>() * 1024);

                    if (dic.TryGetValue("MemAvailable", out str) || dic.TryGetValue("MemFree", out str))
                        retVal.Add(FreeMemory, str.RemovePostFix(" kB").To<int>() * 1024);
                }

                return retVal;
            }

            return null;

        }
    }
}
