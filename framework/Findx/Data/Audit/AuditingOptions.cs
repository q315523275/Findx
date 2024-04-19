namespace Findx.Data;

/// <summary>
///     审计配置
/// </summary>
public class AuditingOptions : IOptions<AuditingOptions>
{
    /// <summary>
    ///     获取或设置 是否启用
    /// </summary>
    public bool Enabled { set; get; }

    /// <summary>
    ///     获取或设置 提取请求参数
    /// </summary>
    public bool ExtractRequestBody { set; get; }

    /// <summary>
    ///     获取或设置 提取返回结果
    /// </summary>
    public bool ExtractResponseBody { set; get; }

    /// <summary>
    ///     审计选项
    /// </summary>
    public AuditingOptions Value => this;
}