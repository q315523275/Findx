using System.Text.Json.Serialization;
using Findx.Messaging;

namespace Findx.Module.ConfigService.Handling;

/// <summary>
///     配置数据改变事件
/// </summary>
public class ConfigDataChangeEvent : IApplicationEvent
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="dataId"></param>
    /// <param name="dataType"></param>
    /// <param name="content"></param>
    /// <param name="environment"></param>
    /// <param name="version"></param>
    [JsonConstructor]
    public ConfigDataChangeEvent(string appId, string dataId, string dataType, string content, string environment,
        long version)
    {
        AppId = appId;
        DataId = dataId;
        DataType = dataType;
        Content = content;
        Environment = environment;
        Version = version;
    }

    /// <summary>
    ///     应用编号
    /// </summary>
    [JsonInclude]
    public string AppId { get; set; }

    /// <summary>
    ///     数据编号
    /// </summary>
    [JsonInclude]
    public string DataId { get; set; }

    /// <summary>
    ///     数据类型
    /// </summary>
    [JsonInclude]
    public string DataType { get; set; }

    /// <summary>
    ///     内容
    /// </summary>
    [JsonInclude]
    public string Content { get; set; }

    /// <summary>
    ///     环境
    /// </summary>
    [JsonInclude]
    public string Environment { get; set; }

    /// <summary>
    ///     版本号
    /// </summary>
    [JsonInclude]
    public long Version { get; set; }
}