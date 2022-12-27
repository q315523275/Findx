using Findx.Utils;

namespace Findx.Machine.Cpu;

/// <summary>
/// Ctor
/// </summary>
public class CpuHelper
{
    /// <summary>
    /// 获取当前系统消耗的 CPU 时间
    /// </summary>
    /// <returns></returns>
    public static CpuTime GetCpuTime()
    {
        if (Common.IsWindows)
            return WindowsCpu.GetCpuTime();
        
        if (Common.IsLinux)
            return LinuxCpu.GetCpuTime();
        
        return new CpuTime();
    }

    /// <summary>
    /// 计算 CPU 使用率
    /// </summary>
    /// <param name="oldTime"></param>
    /// <param name="newTime"></param>
    /// <returns></returns>
    public static double CalculateCpuLoad(CpuTime oldTime, CpuTime newTime)
    {
        var totalTicksSinceLastTime = newTime.SystemTime - oldTime.SystemTime;
        var idleTicksSinceLastTime = newTime.IdleTime - oldTime.IdleTime;

        var ret = 1.0f - ((totalTicksSinceLastTime > 0) ? ((double)idleTicksSinceLastTime) / totalTicksSinceLastTime : 0);

        return ret;
    }
}