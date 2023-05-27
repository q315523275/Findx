using System.Collections.Generic;
using Findx.Security;

namespace Findx.AspNetCore.Mvc;

/// <summary>
///     Mvc功能存储器
/// </summary>
public class MvcFunctionStore : IFunctionStore<MvcFunction>
{
    private readonly List<MvcFunction> _functions = new();

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
}