using System.Threading;
using System.Threading.Tasks;
using Findx.Security;
using Microsoft.Extensions.Hosting;

namespace Findx.AspNetCore.Mvc;

/// <summary>
///     Mvc 功能工作者
/// </summary>
public class MvcFunctionWorker : BackgroundService
{
    /// <summary>
    ///     功能处理器
    /// </summary>
    private readonly IFunctionHandler _functionHandler;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="functionHandler"></param>
    public MvcFunctionWorker(IFunctionHandler functionHandler)
    {
        _functionHandler = functionHandler;
    }

    /// <summary>
    ///     执行
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _functionHandler.InitializeAsync(stoppingToken);
    }
}