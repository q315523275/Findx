#if NET9_0_OR_GREATER
using System.Threading.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;

namespace Findx.Swagger.Transformers;

/// <summary>
///     端点操作扩展
/// </summary>
public static class OperationTransformers
{
    /// <summary>
    ///     端点请求增加Header头信息
    /// </summary>
    /// <param name="options"></param>
    /// <param name="headerName"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static OpenApiOptions AddHeader(this OpenApiOptions options, string headerName, string defaultValue)
    {
        return options.AddOperationTransformer((operation, _, _) =>
        {
            var schema = typeof(string).MapTypeToOpenApiPrimitiveType();
            schema.Default = new OpenApiString(defaultValue);
            operation.Parameters ??= [];
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = headerName,
                In = ParameterLocation.Header,
                Schema = schema
            });
            return Task.CompletedTask;
        });
    }
}
#endif