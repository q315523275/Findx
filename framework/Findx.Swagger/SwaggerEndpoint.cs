using System.Collections.Generic;

namespace Findx.Swagger;

/// <summary>
///     端点组信息
/// </summary>
public class SwaggerEndpoint
{
    /// <summary>
    ///     名称
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     版本
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    ///     文档地址
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///     服务集合
    /// </summary>
    public List<SwaggerEndpointServer> Servers { get; set; } = [];
}