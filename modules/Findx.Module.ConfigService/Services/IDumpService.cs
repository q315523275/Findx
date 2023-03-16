using Findx.Module.ConfigService.Dtos;

namespace Findx.Module.ConfigService.Services;

public interface IDumpService
{
    Task DumpAsync(ConfigDataChangeDto changeDto, CancellationToken cancellationToken = default);
}