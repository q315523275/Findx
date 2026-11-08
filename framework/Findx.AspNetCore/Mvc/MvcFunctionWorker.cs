using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

    private readonly ILogger<MvcFunctionWorker> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public MvcFunctionWorker(IFunctionHandler functionHandler, ILogger<MvcFunctionWorker> logger)
    {
        _functionHandler = functionHandler;
        _logger = logger;
    }

    /// <summary>
    ///     执行
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //  初始化失败不应拖垮宿主（默认 BackgroundService 未捕获异常会 StopHost），改为指数退避重试直至成功或停机
        var delay = TimeSpan.FromSeconds(2);
        var maxDelay = TimeSpan.FromSeconds(60);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _functionHandler.InitializeAsync(stoppingToken);
                return; // 初始化成功即结束
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return; // 正常停机
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MvcFunction 功能信息初始化失败，{DelaySeconds}s 后重试", delay.TotalSeconds);
                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, maxDelay.TotalSeconds));
            }
        }
    }
}
