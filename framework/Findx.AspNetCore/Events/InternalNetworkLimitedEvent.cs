using System;
using Findx.Common;
using Findx.Messaging;

namespace Findx.AspNetCore.Events;

/// <summary>
///     内网请求限定触发事件
/// </summary>
public class InternalNetworkLimitedEvent: IApplicationEvent, IAsync
{
    /// <summary>
    ///     客户端ip
    /// </summary>
    public string ClientIpAddress { set; get; }
    
    /// <summary>
    ///     客户端信息
    /// </summary>
    public string UserAgent { set; get; }
    
    /// <summary>
    ///     触发时间
    /// </summary>
    public DateTime TriggerTime { set; get; }
}