#nullable enable
using System.Text.Json.Serialization;
using Findx.Common;
using Findx.Messaging;

namespace Findx.Data;

/// <summary>
///     审计操作信息
/// </summary>
public class AuditOperationEntry : IApplicationEvent, IAsync
{
#pragma warning disable CS8618
    /// <summary>
    ///     获取或设置 执行的功能名
    /// </summary>
    public string FunctionName { get; set; }

    /// <summary>
    ///     获取或设置 当前租户表示
    /// </summary>
    public string TenantId { get; set; }

    /// <summary>
    ///     获取或设置 当前用户标识
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    ///     获取或设置 当前用户名
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    ///     获取或设置 当前用户昵称
    /// </summary>
    public string NickName { get; set; }

    /// <summary>
    ///     获取或设置 当前访问IP
    /// </summary>
    public string Ip { get; set; }

    /// <summary>
    ///     获取或设置 当前访问UserAgent
    /// </summary>
    public string UserAgent { get; set; }

    /// <summary>
    ///     获取或设置 消息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     获取或设置 信息添加时间
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    ///     获取或设置 结束时间
    /// </summary>
    public DateTime EndedTime { get; set; }

    /// <summary>
    ///     获取或设置 异常对象
    /// </summary>
    [JsonIgnore]
    public Exception? Exception { get; set; }

    /// <summary>
    ///     获取 扩展数据字典
    /// </summary>
    public IDictionary<string, string> ExtraObject { get; } = new Dictionary<string, string>();
#pragma warning restore CS8618
}