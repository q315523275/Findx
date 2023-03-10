using System;
using System.Collections.Generic;

namespace Findx.EventBus
{
    /// <summary>
    /// 原始消息
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public Message()
        {
            Headers = new Dictionary<string, string?>();
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Message(IDictionary<string, string?> headers, object? value)
        {
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            Value = value;
        }

        /// <summary>
        /// Header字典信息
        /// </summary>
        public IDictionary<string, string?> Headers { get; set; }

        /// <summary>
        /// 消息内容体信息
        /// </summary>
        public object? Value { get; set; }
    }
}