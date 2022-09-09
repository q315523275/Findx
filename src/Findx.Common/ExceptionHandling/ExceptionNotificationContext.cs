using System;

namespace Findx.ExceptionHandling
{
    /// <summary>
    /// 异常通知上下文
    /// </summary>
    public class ExceptionNotificationContext
    {
        /// <summary>
        /// 异常对象
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// 是否已处理异常
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// 初始化一个<see cref="ExceptionNotificationContext"/>类型的实例
        /// </summary>
        /// <param name="exception">异常</param>
        /// <param name="handled">是否已处理异常</param>
        public ExceptionNotificationContext(Exception exception, bool handled = true)
        {
            Exception = Check.NotNull(exception, nameof(exception));
            Handled = handled;
        }
    }
}
