using Microsoft.OpenApi;
using Milvasoft.Components.Swagger.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Milvasoft.Components.Swagger.DocumentFilters;

/// <summary>
/// Deletes operations marked with the SwaggerExclude attribute (on the controller, the action or minimal API endpoint metadata) from the swagger documentation.
/// </summary>
public class SwaggerEndpointExcludeFilter : IDocumentFilter
{
    /// <summary>
    /// Applies configuration to swagger document.
    /// </summary>
    /// <param name="swaggerDoc"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (swaggerDoc?.Paths == null)
            return;

        foreach (var contextApiDescription in context.ApiDescriptions)
        {
            var isExcluded = contextApiDescription.ActionDescriptor?.EndpointMetadata?.OfType<SwaggerExcludeAttribute>().Any() == true;

            if (!isExcluded)
                continue;

            var key = string.Concat("/", (contextApiDescription.RelativePath ?? string.Empty).Split('?', 2)[0].TrimStart('/'));

            if (!swaggerDoc.Paths.TryGetValue(key, out var pathItem) || pathItem.Operations == null)
                continue;

            // Remove only the excluded operation, other HTTP methods of the same path stay documented.
            if (contextApiDescription.HttpMethod != null)
                pathItem.Operations.Remove(new HttpMethod(contextApiDescription.HttpMethod));

            if (pathItem.Operations.Count == 0)
                swaggerDoc.Paths.Remove(key);
        }
    }
}
