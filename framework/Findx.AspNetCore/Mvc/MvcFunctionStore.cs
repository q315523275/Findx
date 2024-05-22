using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Findx.Security;

namespace Findx.AspNetCore.Mvc;

/// <summary>
///     Mvc功能存储器
/// </summary>
public class MvcFunctionStore : IFunctionStore<MvcFunction>
{
    private readonly List<MvcFunction> _functions = [];

    /// <summary>
    ///     保存
    /// </summary>
    /// <param name="functions"></param>
    public void SyncToDatabase(IEnumerable<MvcFunction> functions)
    {
        _functions.Clear();
        _functions.AddRange(functions);
    }

    /// <summary>
    ///     查询
    /// </summary>
    /// <returns></returns>
    public IEnumerable<MvcFunction> GetFromDatabase()
    {
        return _functions;
    }

    /// <summary>
    ///     保存
    /// </summary>
    /// <param name="functions"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task SyncToDatabaseAsync(IEnumerable<MvcFunction> functions, CancellationToken cancellationToken = default)
    {
        _functions.Clear();
        _functions.AddRange(functions);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     查询
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<List<MvcFunction>> QueryFromDatabaseAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_functions);
    }
}