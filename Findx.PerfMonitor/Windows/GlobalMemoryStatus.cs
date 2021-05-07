using System.Runtime.InteropServices;

namespace Findx.PerfMonitor.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MemoryStatusEx
    {
        internal uint dwLength;
        private readonly uint dwMemoryLoad;
        internal readonly ulong ullTotalPhys;
        internal readonly ulong ullAvailPhys;
        private readonly ulong ullTotalPageFile;
        private readonly ulong ullAvailPageFile;
        private readonly ulong ullTotalVirtual;
        private readonly ulong ullAvailVirtual;
        private readonly ulong ullAvailExtendedVirtual;
    }

    internal static class GlobalMemoryStatus
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer
        ); //ref. vs. [In, Out], see: https://www.pinvoke.net/default.aspx/kernel32.globalmemorystatusex

        internal static (bool success, ulong total, ulong avail) GetTotalPhysAndAvailPhys()
        {
            var statEx = new MemoryStatusEx { dwLength = (uint)Marshal.SizeOf(typeof(MemoryStatusEx)) };

            return GlobalMemoryStatusEx(ref statEx)
                ? (true, statEx.ullTotalPhys, statEx.ullAvailPhys)
                : (false, 0, 0);
        }
    }
}
