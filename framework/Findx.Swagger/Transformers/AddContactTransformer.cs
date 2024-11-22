#if NET9_0_OR_GREATER
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Swagger.Transformers;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

public sealed class AddContactTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info.Contact = new OpenApiContact
        {
            Name = "OpenAPI Enthusiast",
            Email = "iloveopenapi@example.com"
        };
        return Task.CompletedTask;
    }
}
#endif