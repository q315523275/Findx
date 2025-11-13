using System.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;
#if NET8_0_OR_GREATER
using Microsoft.OpenApi;
#else
using Microsoft.OpenApi.Models;
#endif
namespace Findx.Swagger.Filters;

/// <summary>
///     标签重排过滤器
/// </summary>
public class TagOrderDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        if (document.Tags == null) return;
        
        document.Tags.Clear();

        foreach (var path in document.Paths)
        {
            if (path.Value.Operations != null && path.Value.Operations.Any())
            {
                foreach (var o in path.Value.Operations)
                {
                    if (o.Value.Tags != null && o.Value.Tags.Any())
                    {
                        foreach (var tag in o.Value.Tags)
                        {
                            #if NET8_0_OR_GREATER
                            document.Tags.Add(new OpenApiTag { Name = tag.Name });
                            #else
                            document.Tags.Add(tag);                 
                            #endif
                        }
                    }
                }
            }
        }
    }
}