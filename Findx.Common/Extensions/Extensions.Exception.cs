using System;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Findx.Extensions
{
    /// <summary>
    /// 系统扩展 - 异常
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 透传异常
        /// </summary>
        /// <param name="exception"></param>
        public static void ReThrow(this Exception exception)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }

        /// <summary>
        /// 格式化异常消息
        /// </summary>
        /// <param name="e">异常对象</param>
        /// <param name="isHideStackTrace">是否隐藏异常规模信息</param>
        /// <returns>格式化后的异常信息字符串</returns>
        public static string FormatMessage(this Exception e, bool isHideStackTrace = false)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            string appString = string.Empty;
            while (e != null)
            {
                if (count > 0)
                {
                    appString += "  ";
                }
                sb.AppendLine(string.Format("{0}异常消息：{1}", appString, e.Message));
                sb.AppendLine(string.Format("{0}异常类型：{1}", appString, e.GetType().FullName));
                sb.AppendLine(string.Format("{0}异常方法：{1}", appString, (e.TargetSite == null ? null : e.TargetSite.Name)));
                sb.AppendLine(string.Format("{0}异常源：{1}", appString, e.Source));
                if (!isHideStackTrace && e.StackTrace != null)
                {
                    sb.AppendLine(string.Format("{0}异常堆栈：{1}", appString, e.StackTrace));
                }
                if (e.InnerException != null)
                {
                    sb.AppendLine(string.Format("{0}内部异常：", appString));
                    count++;
                }
                e = e.InnerException;
            }
            return sb.ToString();
        }
    }
}
