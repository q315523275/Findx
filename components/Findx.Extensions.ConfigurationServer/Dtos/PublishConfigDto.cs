using Findx.Data;

namespace Findx.Extensions.ConfigurationServer.Dtos;

/// <summary>
///     发布配置Dto
/// </summary>
public class PublishConfigDto : IRequest
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
    ///     描述
    /// </summary>
    public string Comments { get; set; }

    /// <summary>
    ///     是否Beta(灰度)
    /// </summary>
    public bool IsBeta { get; set; }

    /// <summary>
    ///     灰度限定Ip集合
    /// </summary>
    public string BetaIps { get; set; }
}