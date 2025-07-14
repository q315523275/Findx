using System;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Common;
using Findx.Messaging;

namespace Findx.AspNetCore.Events;

/// <summary>
///     重复请求事件
/// </summary>
public class DuplicateRequestedEvent: IApplicationEvent, IAsync
{
    /// <summary>
    ///     锁定类型
    /// </summary>
    public LockType LockType { set; get; }
    
    /// <summary>
    ///     锁定key
    /// </summary>
    public string LockKey { set; get; }
    
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