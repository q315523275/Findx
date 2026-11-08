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
    ///     是否启用（默认关闭，生产环境需显式配置开启，避免 Swagger 文档意外暴露）
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    ///     额外包含的 XML 注释文件路径清单（相对应用基目录或绝对路径），默认仅加载入口程序集 XML
    /// </summary>
    public ICollection<string> XmlCommentFiles { get; set; } = new List<string>();

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
