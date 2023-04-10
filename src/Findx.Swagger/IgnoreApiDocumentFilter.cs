using Findx.Extensions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Reflection;

namespace Findx.Swagger
{
    /// <summary>
    /// 隐藏Api过滤器
    /// </summary>
    public class IgnoreApiDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var apiDescription in context.ApiDescriptions)
            {
                if (apiDescription.TryGetMethodInfo(out MethodInfo method))
                {
                    if (method.ReflectedType.HasAttribute<IgnoreApiAttribute>() || method.HasAttribute<IgnoreApiAttribute>())
                    {
                        var key = $"/{apiDescription.RelativePath}";
                        if (key.Contains('?'))
                        {
                            var index = key.IndexOf("?", StringComparison.Ordinal);
                            key = key.Substring(0, index);
                        }

                        swaggerDoc.Paths.Remove(key);
                    }
                }
            }
        }
    }
}
