using System.Threading.Tasks;

namespace Findx.Security;

/// <summary>
///     定义功能信息存储器
/// </summary>
/// <typeparam name="TFunction"></typeparam>
public interface IFunctionStore<TFunction>
{
    /// <summary>
    ///     同步功能信息至存储库
    /// </summary>
    /// <param name="functions"></param>
    /// <param name="cancellationToken"></param>
    Task SyncToDatabaseAsync(IEnumerable<TFunction> functions, CancellationToken cancellationToken = default);

    /// <summary>
    ///     从存储器同步功能信息
    /// </summary>
    Task<List<TFunction>> QueryFromDatabaseAsync(CancellationToken cancellationToken = default);
}