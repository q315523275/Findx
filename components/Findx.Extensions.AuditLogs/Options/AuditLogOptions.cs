using Microsoft.Extensions.Options;

namespace Findx.Extensions.AuditLogs.Options;

/// <summary>
///     审计日志模块配置（GxP 完整性）
/// </summary>
public class AuditLogOptions : IOptions<AuditLogOptions>
{
    /// <summary>
    ///     获取或设置 是否启用完整性校验（SHA256 哈希链）。默认关闭以性能优先，GxP 场景需显式开启
    /// </summary>
    public bool EnableIntegrity { set; get; }

    /// <summary>
    ///     获取或设置 哈希算法，当前仅支持 SHA256
    /// </summary>
    public string HashAlgorithm { set; get; } = "SHA256";

    /// <summary>
    ///     options值
    /// </summary>
    public AuditLogOptions Value => this;
}
