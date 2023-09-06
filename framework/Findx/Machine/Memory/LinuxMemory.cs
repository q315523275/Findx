using Findx.Extensions;
using Findx.Utilities;

namespace Findx.Machine.Memory;

/// <summary>
///     Linux系统内存
/// </summary>
public class LinuxMemory
{
    /// <summary>
    ///     获取内存信息
    /// </summary>
    /// <returns></returns>
    public static MemoryValue GetMemory()
    {
        ulong totalPhysicalMemory = 0; // 物理内存字节数
        ulong availablePhysicalMemory = 0; // 可用的物理内存字节数
        ulong usedPercentage = 0; // 已用物理内存百分比
        ulong totalVirtualMemory = 0; // 虚拟内存字节数
        ulong availableVirtualMemory = 0; // 可用虚拟内存字节数

        // 总内存、可用内存
        var dic = CommonUtility.ReadInfo("/proc/meminfo");
        if (dic == null) return default;

        if (dic.TryGetValue("MemTotal", out var str))
            totalPhysicalMemory = str.RemovePostFix(" kB").To<ulong>() * 1024;

        if (dic.TryGetValue("MemAvailable", out str) || dic.TryGetValue("MemFree", out str))
            availablePhysicalMemory = str.RemovePostFix(" kB").To<ulong>() * 1024;

        if (totalPhysicalMemory > 0)
            usedPercentage = (totalPhysicalMemory - availablePhysicalMemory) * 100 / totalPhysicalMemory;

        if (dic.TryGetValue("VmallocTotal", out str))
            totalVirtualMemory = str.RemovePostFix(" kB").To<ulong>() * 1024;

        if (dic.TryGetValue("VmallocUsed", out str))
            availableVirtualMemory = totalVirtualMemory - str.RemovePostFix(" kB").To<ulong>() * 1024;

        return new MemoryValue(totalPhysicalMemory, availablePhysicalMemory, usedPercentage, totalVirtualMemory,
            availableVirtualMemory);
    }
}