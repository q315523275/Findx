using Findx.Module.ConfigService.Dtos;

namespace Findx.Module.ConfigService.Services;

/// <summary>
/// 转储服务
/// </summary>
public interface IDumpService
{
    /// <summary>
    /// 转储
    /// </summary>
    /// <param name="changeDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DumpAsync(ConfigDataChangeDto changeDto, CancellationToken cancellationToken = default);
}