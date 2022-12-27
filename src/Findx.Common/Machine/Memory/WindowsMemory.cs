﻿using System.Runtime.InteropServices;

namespace Findx.Machine.Memory
{
    /// <summary>
    /// Windows系统内存
    /// </summary>
    public class WindowsMemory
    {
#if NET7_0_OR_GREATER
        /// <summary>
        /// 在内存超过 4 GB 的计算机上， GlobalMemoryStatus函数可能返回不正确的信息，报告值 –1 表示溢出。因此，应用程序应改用 GlobalMemoryStatusEx函数。
        /// </summary>
        /// <remarks>Windows XP [仅限桌面应用程序];最低支持服务器 Windows Server 2003 [仅限桌面应用程序]</remarks>
        /// <param name="lpBuffer"></param>
        [LibraryImport("Kernel32.dll", SetLastError = true)]
        public static partial void GlobalMemoryStatus(ref MemoryStatus lpBuffer);

        /// <summary>
        /// 检索有关系统当前使用物理和虚拟内存的信息
        /// </summary>
        /// <remarks><see href="https://docs.microsoft.com/zh-cn/windows/win32/api/sysinfoapi/nf-sysinfoapi-globalmemorystatusex"/></remarks>
        /// <param name="lpBuffer"></param>
        /// <returns></returns>
        [LibraryImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial Boolean GlobalMemoryStatusEx(ref MemoryStatusExe lpBuffer);

#else


        /// <summary>
        /// 在内存超过 4 GB 的计算机上， GlobalMemoryStatus函数可能返回不正确的信息，报告值 –1 表示溢出。因此，应用程序应改用 GlobalMemoryStatusEx函数。
        /// </summary>
        /// <remarks>Windows XP [仅限桌面应用程序];最低支持服务器 Windows Server 2003 [仅限桌面应用程序]</remarks>
        /// <param name="lpBuffer"></param>
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void GlobalMemoryStatus(ref MemoryStatus lpBuffer);

        /// <summary>
        /// 检索有关系统当前使用物理和虚拟内存的信息
        /// </summary>
        /// <remarks><see href="https://docs.microsoft.com/zh-cn/windows/win32/api/sysinfoapi/nf-sysinfoapi-globalmemorystatusex"/></remarks>
        /// <param name="lpBuffer"></param>
        /// <returns></returns>
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern Boolean GlobalMemoryStatusEx(ref MemoryStatusExe lpBuffer);
#endif
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static MemoryValue GetMemory()
        {
            // 检查 Windows 内核版本，是否为旧系统
            if (Environment.OSVersion.Version.Major < 5)
            {
                // https://en.wikipedia.org/wiki/List_of_Microsoft_Windows_versions");
                return default;
            }

            var memoryStatusEx = new MemoryStatusExe();
            // 初始化结构的大小
            memoryStatusEx.Init();
            // 刷新值
            if (!GlobalMemoryStatusEx(ref memoryStatusEx)) return default;

            var totalPhysicalMemory = memoryStatusEx.ullTotalPhys;
            var availablePhysicalMemory = memoryStatusEx.ullAvailPhys;
            var totalVirtualMemory = memoryStatusEx.ullTotalVirtual;
            var availableVirtualMemory = memoryStatusEx.ullAvailVirtual;
            var usedPercentage = memoryStatusEx.dwMemoryLoad;
            return new MemoryValue(
                totalPhysicalMemory,
                availablePhysicalMemory,
                usedPercentage,
                totalVirtualMemory,
                availableVirtualMemory);
        }
    }
}