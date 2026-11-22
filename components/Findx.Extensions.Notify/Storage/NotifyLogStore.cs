using Findx.Extensions.Notify.Entity;
using Findx.NewId;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.Extensions.Notify.Storage;

/// <summary>
///     通知日志存储。落库失败只记警告，绝不因为"日志写不进去"影响通知本身或业务线程
/// </summary>
public class NotifyLogStore
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IKeyGenerator<long> _keyGenerator;
    private readonly ILogger<NotifyLogStore> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public NotifyLogStore(IServiceProvider serviceProvider, IKeyGenerator<long> keyGenerator, ILogger<NotifyLogStore> logger)
    {
        _serviceProvider = serviceProvider;
        _keyGenerator = keyGenerator;
        _logger = logger;
    }

    /// <summary>
    ///     写入一条发送日志（调用方保证已填好最终状态）
    /// </summary>
    /// <param name="log"></param>
    /// <param name="cancellationToken"></param>
    public async Task PersistAsync(NotifyLog log, CancellationToken cancellationToken = default)
    {
        if (log == null) return;

        try
        {
            await _serviceProvider.ExecuteScopedWorkAsync(async sp =>
            {
                var repository = sp.GetRequiredService<IRepository<NotifyLog, long>>();
                if (log.Id == 0) log.Id = _keyGenerator.Create();
                await repository.InsertAsync(log, cancellationToken);
            });
        }
        catch (Exception ex)
        {
            //  通知已经发出去了，日志写失败不能反噬主流程
            _logger.LogWarning(ex, "通知日志落库失败（不影响发送）：Channel={Channel}，Receivers={Receivers}", log.Channel, log.Receivers);
        }
    }
}
