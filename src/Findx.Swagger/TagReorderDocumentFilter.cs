using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Findx.Swagger;

/// <summary>
/// 标签过滤器
/// </summary>
public class TagReorderDocumentFilter: IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags.Clear();
        
        foreach (var path in swaggerDoc.Paths)
        {
            foreach (var o in path.Value.Operations)
            {
                foreach (var tag in o.Value.Tags)
                {
                    swaggerDoc.Tags.Add(tag);
                }
            }
        }
    }
}