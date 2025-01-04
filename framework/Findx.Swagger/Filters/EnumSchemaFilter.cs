#if !NET9_0_OR_GREATER
using System;
using Findx.Extensions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Findx.Swagger.Filters;

/// <summary>
///    枚举字段显示属性、值和描述
/// </summary>
public class EnumSchemaFilter: ISchemaFilter
{
    /// <summary>
    ///     
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            foreach (var name in Enum.GetNames(context.Type))
            {
                var enumValue = Enum.Parse(context.Type, name);
                // Todo 如果经常使用,可以进行一些性能优化
                schema.Enum.Add(new OpenApiString($"{name}({enumValue.GetType().GetField(name).GetDescription()})={enumValue.CastTo<int>()}"));
            }
        }
    }
}
#endif