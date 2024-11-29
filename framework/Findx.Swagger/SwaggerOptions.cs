using System.Collections.Generic;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Findx.Swagger;

public class SwaggerOptions
{
    /// <summary>
    ///     端点集合
    /// </summary>
    public ICollection<SwaggerEndpoint> Endpoints { get; set; } = new List<SwaggerEndpoint>();

    /// <summary>
    ///     是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     所有参数是否启用小驼峰
    /// </summary>
    public bool AllParametersInCamelCase { get; set; } = false;

    /// <summary>
    ///     隐藏模型集合
    /// </summary>
    public bool HideSchemas { get; set; } = true;

    /// <summary>
    ///     文档深度
    /// </summary>
    public DocExpansion DocExpansion { get; set; } = DocExpansion.None;
}
