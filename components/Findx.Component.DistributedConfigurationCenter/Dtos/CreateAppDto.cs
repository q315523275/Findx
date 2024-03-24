using Findx.Data;

namespace Findx.Component.DistributedConfigurationCenter.Dtos;

/// <summary>
///     创建App Dto
/// </summary>
public class CreateAppDto: IRequest
{
    /// <summary>
    ///     应用名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     appid
    /// </summary>
    public string AppId { get; set; }

    /// <summary>
    ///     密钥
    /// </summary>
    public string Secret { get; set; }
}