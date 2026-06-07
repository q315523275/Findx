namespace Findx.Module.EleAdminPlus.Internal;

/// <summary>
///     平台管理
/// </summary>
public static class PlatformManager
{
    private static readonly AsyncLocal<long> ValueAccessor = new();

    /// <summary>
    ///     当前帐套编号
    /// </summary>
    public static long Current
    {
        get => ValueAccessor.Value;
        set => ValueAccessor.Value = value;
    }  
}