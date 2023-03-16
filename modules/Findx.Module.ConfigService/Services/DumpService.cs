using Findx.Module.ConfigService.Client;
using Findx.Module.ConfigService.Dtos;

namespace Findx.Module.ConfigService.Services;

public class DumpService: IDumpService
{
    private readonly IClientCallBack _clientCallBack;

    public DumpService(IClientCallBack clientCallBack)
    {
        _clientCallBack = clientCallBack;
    }

    public Task DumpAsync(ConfigDataChangeDto changeDto, CancellationToken cancellationToken = default)
    {
        _clientCallBack.CallBack($"{changeDto.AppId}-{changeDto.Environment}", changeDto);

        return Task.CompletedTask;
    }
}