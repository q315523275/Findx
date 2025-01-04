#if !NET9_0_OR_GREATER
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Findx.Swagger.Filters;

/// <summary>
///     标签重排过滤器
/// </summary>
public class TagReorderDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags.Clear();

        foreach (var path in swaggerDoc.Paths)
        foreach (var o in path.Value.Operations)
        foreach (var tag in o.Value.Tags)
            swaggerDoc.Tags.Add(tag);
    }
}
#endif