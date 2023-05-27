using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Findx.Machine.Cpu;

/// <summary>
/// </summary>
public class LinuxCpu
{
    /// <summary>
    ///     获取 CPU 时间
    /// </summary>
    /// <returns></returns>
    public static CpuTime GetCpuTime()
    {
        ulong idleTime = 0;
        ulong systemTime = 0;
        var file = "/proc/stat";

        if (!File.Exists(file)) return new CpuTime(idleTime, systemTime);

        try
        {
            var text = File.ReadAllLines(file);

            foreach (var item in text)
            {
                if (!item.StartsWith("cpu")) continue;
                var values = item.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToArray();
                systemTime += (ulong)values[1..].Select(decimal.Parse).Sum();
                idleTime += ulong.Parse(values[4]);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            Debug.Assert(false, ex.Message);
            throw new PlatformNotSupportedException(
                $"{RuntimeInformation.OSArchitecture.ToString()}    {Environment.OSVersion.Platform.ToString()} {Environment.OSVersion}");
        }

        return new CpuTime(idleTime, systemTime);
    }
}