namespace Findx.PerfMonitor
{
    public class helper
    {
        const ulong GB = 1024 * 1024 * 1024;//定义GB的计算常量
        const ulong MB = 1024 * 1024;//定义MB的计算常量
        const ulong KB = 1024;//定义KB的计算常量
        public static string ByteConversionGBMBKB(ulong KSize)
        {
            if (KSize / GB >= 1)//如果当前Byte的值大于等于1GB
                return (Math.Round(KSize / (float)GB, 2)).ToString() + "GB";//将其转换成GB
            else if (KSize / MB >= 1)//如果当前Byte的值大于等于1MB
                return (Math.Round(KSize / (float)MB, 2)).ToString() + "MB";//将其转换成MB
            else if (KSize / KB >= 1)//如果当前Byte的值大于等于1KB
                return (Math.Round(KSize / (float)KB, 2)).ToString() + "KB";//将其转换成KGB
            else
                return KSize.ToString() + "Byte";//显示Byte值
        }
    }
}
