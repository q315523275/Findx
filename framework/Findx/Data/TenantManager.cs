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