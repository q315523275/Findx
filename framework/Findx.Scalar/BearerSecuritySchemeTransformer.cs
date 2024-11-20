using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Findx.Scalar;

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider): IOpenApiDocumentTransformer 
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken) 
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer")) 
        {
            // Add the security scheme at the document level
            var requirements = new Dictionary<string, OpenApiSecurityScheme> 
            {
                ["Bearer"] = new() {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // "bearer" refers to the header name here
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token"
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;
 
            // Apply it as a requirement for all operations
            foreach(var operation in document.Paths.Values.SelectMany(path => path.Operations)) 
            {
                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
                    }] = Array.Empty < string > ()
                });
            }
        }
        
        document.Info = new OpenApiInfo
        {
            Title = "My API Bearer scheme",
            Version = "eleAdmin",
            Description = "API for Damien"
        };
    }
}