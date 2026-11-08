using System.Threading;
using System.Threading.Tasks;

namespace Findx.Data;

/// <summary>
///     审计完整性校验服务：按 Id 升序重算每条审计记录的哈希并与库中值比对，同时校验链衔接
/// </summary>
public interface IAuditIntegrityValidator
{
    /// <summary>
    ///     校验全链
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<AuditIntegrityValidationResult> ValidateAsync(CancellationToken cancellationToken = default);
}
