using System.Runtime.InteropServices;

namespace Findx.Machine.Cpu;

/// <summary>
/// 
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct FileTime
{
    /// <summary>
    /// 时间的低位部分
    /// </summary>
    public uint DateTimeLow;

    /// <summary>
    /// 时间的高位部分
    /// </summary>
    public uint DateTimeHigh;
}