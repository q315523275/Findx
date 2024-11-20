using System.Collections.Generic;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Findx.Swagger;

public class SwaggerOptions
{
    public ICollection<SwaggerEndpoint> Endpoints { get; set; } = new List<SwaggerEndpoint>();

    public bool Enabled { get; set; } = true;

    public bool AllParametersInCamelCase { get; set; } = false;

    public bool HideSchemas { get; set; } = true;

    public DocExpansion DocExpansion { get; set; } = DocExpansion.None;
}
