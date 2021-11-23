using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Findx.Utils
{
    /// <summary>
    /// 计量工具类
    /// </summary>
    public static class MetricsUtil
    {
        /// <summary>
        /// 获取Cpu使用率
        /// </summary>
        /// <returns></returns>
        public static async Task<double> GetCpuUsageForProcess()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

            await Task.Delay(500);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;

            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            return cpuUsageTotal * 100;
        }
    }
}
