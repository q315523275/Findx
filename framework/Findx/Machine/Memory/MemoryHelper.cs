using Findx.Utils;

namespace Findx.Machine.Memory;

/// <summary>
///     内存工具类
/// </summary>
public static class MemoryHelper
{
    /// <summary>
    ///     获取当前系统的内存信息
    /// </summary>
    /// <returns></returns>
    public static MemoryValue GetMemoryValue()
    {
        if (Common.IsWindows)
            return WindowsMemory.GetMemory();

        if (Common.IsLinux)
            return LinuxMemory.GetMemory();

        return default;
    }
}