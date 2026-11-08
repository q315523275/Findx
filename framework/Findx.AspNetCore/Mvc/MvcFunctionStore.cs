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
    //  单例被请求线程并发读取。采用不可变快照：写入时整体替换引用，读取端永远拿到一致（可能略旧）的列表，
    //  避免边 Clear/AddRange 边枚举导致的 CollectionModified 异常。
    private volatile List<MvcFunction> _functions = [];

    /// <summary>
    ///     保存
    /// </summary>
    /// <param name="functions"></param>
    public void SyncToDatabase(List<MvcFunction> functions)
    {
        _functions = functions ?? [];
    }

    /// <summary>
    ///     保存
    /// </summary>
    /// <param name="functions"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task SyncToDatabaseAsync(List<MvcFunction> functions, CancellationToken cancellationToken = default)
    {
        _functions = functions ?? [];
        return Task.CompletedTask;
    }

    /// <summary>
    ///     查询
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<List<MvcFunction>> QueryFromDatabaseAsync(CancellationToken cancellationToken = default)
    {
        //  返回副本，外部修改不影响内部快照
        return Task.FromResult(_functions);
    }
}
