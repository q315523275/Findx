using System.Collections.Generic;
using Findx.Discovery.Abstractions;

namespace Findx.Discovery.Internals;

/// <summary>
///     配置方式服务端点实体
/// </summary>
public class ConfigServiceEndPoint: IServiceEndPoint
{
    /// <summary>
    ///     服务名称
    /// </summary>
    public string ServiceName { get; set; }
    
    /// <summary>
    ///     主机
    /// </summary>
    public string Host { get; set; }
    
    /// <summary>
    ///     端口
    /// </summary>
    public int Port { get; set; }
    
    /// <summary>
    ///     元数据
    /// </summary>
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}