using System.Threading;
using System.Threading.Tasks;
using Findx.Component.DistributedConfigurationCenter.Client;
using Findx.Component.DistributedConfigurationCenter.Dtos;

namespace Findx.Component.DistributedConfigurationCenter.Services;

/// <summary>
///     转储服务
/// </summary>
public class DumpService : IDumpService
{
    private readonly IClientCallBack _clientCallBack;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="clientCallBack"></param>
    public DumpService(IClientCallBack clientCallBack)
    {
        _clientCallBack = clientCallBack;
    }

    /// <summary>
    ///     转储
    /// </summary>
    /// <param name="changeDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task DumpAsync(ConfigDataChangeDto changeDto, CancellationToken cancellationToken = default)
    {
        _clientCallBack.CallBack($"{changeDto.AppId}-{changeDto.Environment}", changeDto);

        return Task.CompletedTask;
    }
}