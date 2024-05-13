using Findx.Data;

namespace Findx.Extensions.ConfigurationServer.Dtos;

/// <summary>
///     查询App Dto
/// </summary>
public class QueryAppDto: PageBase
{
    /// <summary>
    ///     应用名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     appid
    /// </summary>
    public string AppId { get; set; }
}