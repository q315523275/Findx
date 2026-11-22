namespace Findx.Extensions.WorkflowCore.Engine;

/// <summary>
///     业务单据适配器注册表：按 BusinessType 找实现，找不到返回 null 由调用方按"无业务绑定"处理
/// </summary>
public class FlwBusinessAdapterRegistry
{
    private readonly Dictionary<string, IFlwBusinessAdapter> _adapters = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="adapters">由 DI 注入的全部适配器实现，可为空集合</param>
    public FlwBusinessAdapterRegistry(IEnumerable<IFlwBusinessAdapter> adapters)
    {
        if (adapters == null) return;
        foreach (var adapter in adapters)
        {
            if (adapter == null || string.IsNullOrWhiteSpace(adapter.BusinessType)) continue;
            _adapters[adapter.BusinessType] = adapter;
        }
    }

    /// <summary>
    ///     获取适配器，未注册返回 null
    /// </summary>
    public IFlwBusinessAdapter Get(string businessType)
    {
        if (string.IsNullOrWhiteSpace(businessType)) return null;
        return _adapters.TryGetValue(businessType, out var adapter) ? adapter : null;
    }

    /// <summary>
    ///     是否已注册该业务类型
    /// </summary>
    public bool Exists(string businessType)
    {
        return Get(businessType) != null;
    }

    /// <summary>
    ///     已注册的业务类型集合
    /// </summary>
    public IReadOnlyCollection<string> BusinessTypes => _adapters.Keys;

    /// <summary>
    ///     已注册的适配器集合（审批管理选择业务模块时枚举）
    /// </summary>
    public IReadOnlyCollection<IFlwBusinessAdapter> Adapters => _adapters.Values;
}
