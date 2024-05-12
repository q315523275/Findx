using Findx.Data;

namespace Findx.Extensions.ConfigurationServer.Dtos;

/// <summary>
///     配置数据变更通知
/// </summary>
public class ConfigDataChangeDto : IRequest
{
    /// <summary>
    ///     应用编号
    /// </summary>
    public string AppId { get; set; }

    /// <summary>
    ///     数据编号
    /// </summary>
    public string DataId { get; set; }

    /// <summary>
    ///     数据类型
    /// </summary>
    public string DataType { get; set; }

    /// <summary>
    ///     内容
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    ///     环境
    /// </summary>
    public string Environment { get; set; }

    /// <summary>
    ///     版本号
    /// </summary>
    public long Version { get; set; }
}