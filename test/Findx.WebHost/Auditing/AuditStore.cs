using System.Threading;
using System.Threading.Tasks;
using Findx.Data;
using Findx.DependencyInjection;
using Findx.Extensions;
using Microsoft.Extensions.Logging;

namespace Findx.WebHost.Auditing;

/// <summary>
///     审计存储
/// </summary>
public class AuditStore : IAuditStore, ISingletonDependency
{
    private readonly ILogger<AuditStore> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="logger"></param>
    public AuditStore(ILogger<AuditStore> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     保存
    /// </summary>
    /// <param name="operationEntry"></param>
    /// <param name="cancelToken"></param>
    /// <returns></returns>
    public Task SaveAsync(AuditOperationEntry operationEntry, CancellationToken cancelToken = default)
    {
        //_logger.LogInformation(operationEntry.ToJson());
        return Task.CompletedTask;
    }
}