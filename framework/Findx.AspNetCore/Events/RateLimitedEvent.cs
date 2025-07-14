using System;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Common;
using Findx.Messaging;

namespace Findx.AspNetCore.Events;

/// <summary>
///     速率限定触发
/// </summary>
public class RateLimitedEvent: IApplicationEvent, IAsync
{
    /// <summary>
    ///     业务标识
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    ///     限定请求时长
    /// </summary>
    public string Period { get; set; }

    /// <summary>
    ///     限定时长内请求次数
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    ///     是否分布式
    /// </summary>
    public bool IsDistributed { set; get; }
    
    /// <summary>
    ///     限速类型
    /// </summary>
    public RateLimitType Type { get; set; }
    
    /// <summary>
    ///     请求次数
    /// </summary>
    public long RequestedTimes { get; set; }
    
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