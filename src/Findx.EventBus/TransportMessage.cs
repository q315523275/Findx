using System;
using System.Collections.Generic;

namespace Findx.EventBus
{
    /// <summary>
    /// 传输内容
    /// </summary>
    public class TransportMessage
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        public TransportMessage(IDictionary<string, string?> headers, ReadOnlyMemory<byte> body)
        {
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            Body = body;
        }
        
        /// <summary>
        /// 事件内容Header信息
        /// </summary>
        public IDictionary<string, string?> Headers { get; }

        /// <summary>
        /// 事件内容
        /// </summary>
        public ReadOnlyMemory<byte> Body { get; }
        
        /// <summary>
        /// 获取事件编号
        /// </summary>
        /// <returns></returns>
        public string GetId()
        {
            return Headers[HeaderConst.MessageId]!;
        }

        /// <summary>
        /// 事件名称
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return Headers[HeaderConst.MessageName]!;
        }

        /// <summary>
        /// 事件分组
        /// </summary>
        /// <returns></returns>
        public string? GetGroup()
        {
            return Headers.TryGetValue(HeaderConst.Group, out var value) ? value : null;
        }

        /// <summary>
        /// 事件关联编号
        /// </summary>
        /// <returns></returns>
        public string? GetCorrelationId()
        {
            return Headers.TryGetValue(HeaderConst.CorrelationId, out var value) ? value : null;
        }
    }
    
    /// <summary>
    /// 传输Headers Key
    /// </summary>
    public static class HeaderConst
    {
        /// <summary>
        /// 事件消息编号
        /// </summary>
        public const string MessageId = "msg-id";

        /// <summary>
        /// 事件消息名称
        /// </summary>
        public const string MessageName = "msg-name";

        /// <summary>
        /// 事件消息组
        /// </summary>
        public const string Group = "msg-group";
        
        /// <summary>
        /// 关联编号
        /// </summary>
        public const string CorrelationId = "correlation-id";
    }
}