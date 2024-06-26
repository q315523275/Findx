using Findx.Data;

namespace Findx.Extensions.ConfigurationServer.Dtos;

/// <summary>
///     查询配置模型
/// </summary>
public class QueryConfigDataDto : PageBase
{
    /// <summary>
    ///     数据编号
    /// </summary>
    public string DataId { set; get; }

    /// <summary>
    ///     应用名称
    /// </summary>
    public string AppId { get; set; }

    /// <summary>
    ///     环境
    /// </summary>
    public string Environment { get; set; }
}