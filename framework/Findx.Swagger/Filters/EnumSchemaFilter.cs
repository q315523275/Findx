using System;
using Findx.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;
#if NET8_0_OR_GREATER
using Microsoft.OpenApi;
#else
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
#endif
namespace Findx.Swagger.Filters;

/// <summary>
///    枚举字段显示属性、值和描述
/// </summary>
public class EnumSchemaFilter: ISchemaFilter
{
    #if NET8_0_OR_GREATER
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum && schema.Enum != null)
        {
            schema.Enum.Clear();
            foreach (var name in Enum.GetNames(context.Type))
            {
                var enumValue = Enum.Parse(context.Type, name);
                var field = context.Type.GetField(name);
                var description = field.GetDescription();
                schema.Enum.Add($"{name}({description})={enumValue.CastTo<int>()}");
            }
        }
    }
    #else
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            foreach (var name in Enum.GetNames(context.Type))
            {
                var enumValue = Enum.Parse(context.Type, name);
                var field = context.Type.GetField(name);
                var description = field.GetDescription();
                schema.Enum.Add(new OpenApiString($"{name}({description})={enumValue.CastTo<int>()}"));
            }
        }
    }
    #endif
}