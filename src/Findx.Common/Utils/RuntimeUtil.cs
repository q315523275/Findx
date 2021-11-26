using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Findx.Utils
{
    /// <summary>
    /// 命令行工具
    /// </summary>
    public static class RuntimeUtil
    {
        /// <summary>
        /// 执行系统命令
        /// </summary>
        /// <param name="exePath"></param>
        /// <returns></returns>
        public static List<string> ExecForStringList(string exePath)
        {
            var results = new List<string>();

            using (var executor = new ProcessExecutor(exePath))
            {
                executor.OnOutputDataReceived += (sender, str) =>
                {
                    results.Add(str);
                };
                executor.Execute();
            }

            return results;
        }

        /// <summary>
        /// 执行系统命令,命令带参数
        /// </summary>
        /// <param name="exePath"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static List<string> ExecForStringList(string exePath, string arguments)
        {
            var results = new List<string>();

            using (var executor = new ProcessExecutor(exePath, arguments))
            {
                executor.OnOutputDataReceived += (sender, str) =>
                {
                    results.Add(str);
                };
                executor.Execute();
            }

            return results;
        }

        /// <summary>
        /// 销毁进程
        /// </summary>
        public static void Destroy()
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// 进程退出钩子
        /// </summary>
        /// <param name="hook"></param>
        public static void AddShutdownHook(Action hook)
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => hook();
        }

        /// <summary>
        /// 可用的处理器数量（一般为CPU核心数）
        /// </summary>
        /// <returns></returns>
        public static int GetProcessorCount()
        {
            return Environment.ProcessorCount;
        }

        /// <summary>
        /// 获取当前进程ID
        /// </summary>
        /// <returns></returns>
        public static int GetPid()
        {
            return Process.GetCurrentProcess().Id;
        }

        /// <summary>
        /// 获取Cpu使用率
        /// </summary>
        /// <returns></returns>
        public static async Task<double> GetCpuUsage()
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
