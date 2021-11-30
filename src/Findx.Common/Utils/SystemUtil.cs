using Findx.Extensions;
using System.Collections.Generic;

namespace Findx.Utils
{
    /// <summary>
    /// 系统工具类
    /// </summary>
    public class SystemUtil
    {
        /// <summary>
        /// 获取机器信息
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string, object> GetMachineInfo()
        {
            var retVal = new Dictionary<string, object>();

            if (Common.IsLinux)
            {
                // 总内存、可用内存
                var dic = Common.ReadInfo("/proc/meminfo");
                if (dic != null)
                {
                    if (dic.TryGetValue("MemTotal", out var str))
                        retVal.Add("TotalMemory", str.RemovePostFix(" kB").To<double>());

                    if (dic.TryGetValue("MemAvailable", out str) || dic.TryGetValue("MemFree", out str))
                        retVal.Add("FreeMemory", str.RemovePostFix(" kB").To<double>());
                }
            }

            return retVal;
        }
    }
}
