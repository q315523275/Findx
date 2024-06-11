namespace Findx.Tracing;

/// <summary>
///     跟踪Id配置属性
/// </summary>
public class CorrelationIdOptions: IOptions<CorrelationIdOptions>
{
    /// <summary>
    ///     Http请求头名称
    /// </summary>
    public string HttpHeaderName { get; set; } = "X-Correlation-Id";

    /// <summary>
    ///     是否将跟踪标识设置在响应头
    /// </summary>
    public bool SetResponseHeader { get; set; } = true;

    /// <summary>
    ///     value
    /// </summary>
    public CorrelationIdOptions Value { get; }
}