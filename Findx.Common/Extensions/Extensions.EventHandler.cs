using System;

namespace Findx.Extensions
{
    /// <summary>
    /// 系统扩展 - 事件
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 安全执行事件处理
        /// </summary>
        /// <param name="eventHandler">The event handler</param>
        /// <param name="sender">Source of the event</param>
        public static void InvokeSafely(this EventHandler eventHandler, object sender)
        {
            eventHandler.InvokeSafely(sender, EventArgs.Empty);
        }

        /// <summary>
        /// 安全执行事件
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void InvokeSafely(this EventHandler eventHandler, object sender, EventArgs e)
        {
            eventHandler?.Invoke(sender, e);
        }

        /// <summary>
        /// 安全执行事件
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="eventHandler"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void InvokeSafely<TEventArgs>(this EventHandler<TEventArgs> eventHandler, object sender, TEventArgs e) where TEventArgs : EventArgs
        {
            eventHandler?.Invoke(sender, e);
        }
    }
}
