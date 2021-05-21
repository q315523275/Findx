using System.Diagnostics;
using System.Reflection;

namespace Findx.Extensions
{
    /// <summary>
    /// 系统扩展 - 程序集
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 获取程序集的文件版本
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string GetFileVersion(this Assembly assembly)
        {
            return FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
        }

        /// <summary>
        /// 获取程序集的产品版本
        /// </summary>
        public static string GetProductVersion(this Assembly assembly)
        {
            return FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
        }
    }
}
