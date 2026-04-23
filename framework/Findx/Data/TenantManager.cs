using Findx.Extensions;

namespace Findx.Data;

/// <summary>
///     租户管理
/// </summary>
public static class TenantManager
{
    private static readonly AsyncLocal<string> ValueAccessor = new();

    /// <summary>
    ///     当前租户编号
    /// </summary>
    public static string Current
    {
        get => ValueAccessor.Value;
        set => ValueAccessor.Value = value;
    }
}

/// <summary>
///     租户管理(范型)
/// </summary>
public static class TenantManager<TKey>
{
    /// <summary>
    ///     当前租户编号
    /// </summary>
    public static TKey Current => TenantManager.Current.IsNotNullOrWhiteSpace() ? TenantManager.Current.CastTo<TKey>() : default;
}