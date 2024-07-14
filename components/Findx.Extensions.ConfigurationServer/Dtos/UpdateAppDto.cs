using Findx.Data;

namespace Findx.Extensions.ConfigurationServer.Dtos;

/// <summary>
/// 更新App Dto
/// </summary>
public class UpdateAppDto: CreateAppDto, IRequest<long>
{
    /// <summary>
    ///     主键id
    /// </summary>
    public long Id { get; set; }
}