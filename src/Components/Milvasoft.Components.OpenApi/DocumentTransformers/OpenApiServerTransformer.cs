using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Milvasoft.Components.OpenApi.Attributes;

namespace Milvasoft.Components.OpenApi.DocumentTransformers;

/// <summary>
/// Deletes properties marked with the <see cref="OpenApiExcludeAttribute"/> attribute from the openapi documentation.
/// </summary>
public class OpenApiServerTransformer : IOpenApiDocumentTransformer
{
    ///  <inheritdoc/>
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        // Generally, the current server URL is derived from the incoming request.
        var currentUrl = document.Servers.FirstOrDefault()?.Url;

        // If the URL is empty or null, manually provide a host (e.g., localhost)
        string host = "localhost:5000";

        if (!string.IsNullOrEmpty(currentUrl))
        {
            // We clean the "http://" or "https://" prefixes and get the pure domain/host
            host = currentUrl.Replace("http://", "").Replace("https://", "").TrimEnd('/');
        }

        // Define both HTTP and HTTPS servers
        document.Servers =
        [
            new OpenApiServer
            {
                Url = $"https://{host}",
                Description = "Secure Server (HTTPS)"
            },
            new OpenApiServer
            {
                Url = $"http://{host}",
                Description = "Insecure Server (HTTP)"
            }
        ];

        return Task.CompletedTask;
    }
}
