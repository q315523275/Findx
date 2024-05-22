using System.Threading.Tasks;
using Findx.Data;
using Findx.Logging;

namespace Findx.Security;

/// <summary>
///     功能信息处理基类
/// </summary>
public abstract class FunctionHandlerBase<TFunction> : IFunctionHandler where TFunction : class, IEntity<long>, IFunction, new()
{
    private readonly Dictionary<string, TFunction> _functionDict = new(StringComparer.OrdinalIgnoreCase);
    private readonly StartupLogger _startupLogger;
    private readonly IFunctionStore<TFunction> _store;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="store"></param>
    /// <param name="startupLogger"></param>
    protected FunctionHandlerBase(IFunctionStore<TFunction> store, StartupLogger startupLogger)
    {
        _store = store;
        _startupLogger = startupLogger;
    }

    /// <summary>
    ///     从程序集中获取功能信息（如MVC的Controller-Action）
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var functions = GetFunctions();
        await _store.SyncToDatabaseAsync(functions, cancellationToken);
        await RefreshCacheAsync(cancellationToken);
    }

    /// <summary>
    ///     刷新功能信息缓存
    /// </summary>
    public async Task RefreshCacheAsync(CancellationToken cancellationToken = default)
    {
        _functionDict.Clear();
        var rows = await _store.QueryFromDatabaseAsync(cancellationToken);
        foreach (var item in rows)
        {
            _functionDict[$"{item.Area}_{item.Controller}_{item.Action}_{item.HttpMethod}"] = item;
        }
        _startupLogger.LogInformation($"刷新功能信息缓存，从数据库获取到 {_functionDict.Count} 个功能信息", GetType().Name);
    }

    /// <summary>
    ///     清空功能信息缓存
    /// </summary>
    public Task ClearCacheAsync(CancellationToken cancellationToken = default)
    {
        _functionDict.Clear();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     查找指定条件的功能信息
    /// </summary>
    /// <param name="area">区域</param>
    /// <param name="controller">控制器</param>
    /// <param name="action">功能方法</param>
    /// <param name="method">方法</param>
    /// <returns>功能信息</returns>
    public IFunction GetFunction(string area, string controller, string action, string method)
    {
        return _functionDict.GetValueOrDefault($"{area}_{controller}_{action}_{method}");
    }

    /// <summary>
    ///     获取所有功能
    /// </summary>
    /// <param name="fromCache"></param>
    /// <returns></returns>
    protected abstract List<TFunction> GetFunctions(bool fromCache = true);
}